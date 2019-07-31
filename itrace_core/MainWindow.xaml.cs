using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using iTrace_Core.Properties;

namespace iTrace_Core
{
    public partial class MainWindow : Window
    {
        TrackerManager TrackerManager;
        Recorder rec;
        ReticleController reticleController;
        SocketServer socketServer;
        WebSocketServer webSocketServer;
        XMLGazeDataWriter xmlGazeDataWriter;
        SessionSetupWindow sessionInformation;
        List<Setting> settings;   

        class Setting
        {
            public Setting(string option)
            {
                Option = option;
            }
            
            public string Option { get; }
            public string Value { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            TrackerManager = new TrackerManager();

            // Initialize Session
            SessionManager.GetInstance().SetScreenDimensions(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);

            // Default the session to the last used output directory and empty everything else
            SessionManager.GetInstance().SetupSession("", "", "", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            SessionManager.GetInstance().ClearCalibration();
            
            xmlGazeDataWriter = new XMLGazeDataWriter();

            InitializeSettingsGrid();
        }
        private void ApplicationLoaded(object sender, RoutedEventArgs e)
        {
            RefreshTrackerList();
            socketServer = new SocketServer();
            webSocketServer = new WebSocketServer();
        }

        private void InitializeSettingsGrid()
        {
            settings = new List<Setting>()
            {
                new Setting("socket_port") { Value = Settings.Default.socket_port.ToString() },
                new Setting("websocket_port") { Value = Settings.Default.websocket_port.ToString() }
            };

            settingsDataGrid.ItemsSource = settings;
        }

        private void ApplySettings(object sender, RoutedEventArgs e)
        {
            int socketPort = 0;
            int websocketPort = 0;
            if (! (int.TryParse(settings[0].Value, out socketPort) && int.TryParse(settings[1].Value, out websocketPort)) )
            {
                MessageBox.Show("Port values must be numeric!", "Invalid Port Value", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (socketPort == websocketPort)
            {
                MessageBox.Show("Port values cannot be the same value!", "Duplicate Port Values", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // These are separate in the event that the max values are actually different
            if (! ((socketPort <= SocketServer.MAX_PORT_NUM) && (socketPort >= SocketServer.MIN_PORT_NUM)) )
            {
                MessageBox.Show("Socket port values must be in the range: " + SocketServer.MIN_PORT_NUM + "-" + SocketServer.MAX_PORT_NUM + "!", "Invalid Socket Port Value", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (! ((websocketPort <= WebSocketServer.MAX_WEBSOCKET_PORT_NUM) && (websocketPort >= WebSocketServer.MIN_WEBSOCKET_PORT_NUM)) )
            {
                MessageBox.Show("Websocket port values must be in the range: " + WebSocketServer.MIN_WEBSOCKET_PORT_NUM + "-" + WebSocketServer.MAX_WEBSOCKET_PORT_NUM + "!", "Invalid Websocket Port Value", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
                        
            Settings.Default.socket_port = Convert.ToInt32(settings[0].Value);
            Settings.Default.websocket_port = Convert.ToInt32(settings[1].Value);
            Settings.Default.Save();
        }

        // Cleanup for when core closes
        private void ApplicationClosed(object sender, EventArgs e)
        {
            if (TrackerManager.Running())
            {
                TrackerManager.StopTracker();
            }

            if (sessionInformation != null && sessionInformation.IsLoaded)
            {
                sessionInformation.Close();
            }

            if (reticleController != null)
                reticleController.Close();

            if (xmlGazeDataWriter.Writing)
                xmlGazeDataWriter.StopWriting();
        }

        private void MenuExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TrackerListChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TrackerList.SelectedIndex >= 0)
            {
                System.Console.WriteLine(TrackerList.SelectedItem.ToString());
                TrackerManager.SetActiveTracker(TrackerList.SelectedItem.ToString());
                if (TrackerList.SelectedItem.ToString() == "Mouse")
                {
                    ActivateCalibrationButton.IsEnabled = false;
                    ShowEyeStatusButton.IsEnabled = false;
                }
                else
                {
                    ActivateCalibrationButton.IsEnabled = true;
                    ShowEyeStatusButton.IsEnabled = true;
                }
            }
        }

        private void RefreshAttachedTrackers(object sender, RoutedEventArgs e)
        {
            RefreshTrackerList();
        }

        private void RefreshTrackerList()
        {
            TrackerManager.FindTrackers();
            TrackerList.ItemsSource = TrackerManager.GetAttachedTrackers();

            /* Default back to mouse tracker
                Nearly all setups should have a pointing device installed meaning this prevents having
                to perform additional UI checks to make sure the user can't start a tracking session
                without a tracker.
            */
            TrackerList.SelectedIndex = 0;
        }

        private void StartTracker(object sender, RoutedEventArgs e)
        {
            if (TrackerManager.Running())
            {
                SessionManager.GetInstance().StopSession();

                ActivateTrackerButton.Content = Properties.Resources.StartTracking;
                TrackerManager.StopTracker();

                socketServer.SendEndSession();
                webSocketServer.SendEndSession();

                if (CheckScreenCap.IsChecked.HasValue && CheckScreenCap.IsChecked.Value)
                {
                    rec.Dispose();
                }

                xmlGazeDataWriter.StopWriting();

                if (TrackerManager.GetActiveTracker().GetTrackerName() != "Mouse")
                {
                    ActivateCalibrationButton.IsEnabled = true;
                    ShowEyeStatusButton.IsEnabled = true;
                }
                TrackerList.IsEnabled = true;
                TrackerRefreshButton.IsEnabled = true;
                SessionSetupButton.IsEnabled = true;
                settingsDataGrid.IsEnabled = true;
                ApplyButton.IsEnabled = true;
                CheckScreenCap.IsEnabled = true;
            }
            else
            {
                string trackerID = TrackerManager.GetActiveTracker().GetTrackerName() + TrackerManager.GetActiveTracker().GetTrackerSerialNumber();
                if (trackerID != SessionManager.GetInstance().CalibratedTrackerID)
                    SessionManager.GetInstance().ClearCalibration();

                //Start the session (SHOULD HAPPPEN FIRST)
                SessionManager.GetInstance().StartSession();

                //Name of .avi hardcoded for now
                if (CheckScreenCap.IsChecked.HasValue && CheckScreenCap.IsChecked.Value)
                {
                    rec = new Recorder(new RecorderParams(SessionManager.GetInstance().DataRootDir + "/screen_rec" + "-" + SessionManager.GetInstance().CurrentSessionTimeStamp + ".avi", 10, SharpAvi.KnownFourCCs.Codecs.MotionJpeg, 80)); //screenrecording start
                    SessionManager.GetInstance().GenerateScreenRecordingStart();
                }

                // Load previously use directory for data storage or the current users desktop directory
                xmlGazeDataWriter.StartWriting(SessionManager.GetInstance().DataRootDir);

                socketServer.SendSessionData();
                webSocketServer.SendSessionData();
                
                ActivateTrackerButton.Content = Properties.Resources.StopTracking;

                TrackerManager.StartTracker();
                ActivateCalibrationButton.IsEnabled = false;
                ShowEyeStatusButton.IsEnabled = false;
                TrackerList.IsEnabled = false;
                TrackerRefreshButton.IsEnabled = false;
                SessionSetupButton.IsEnabled = false;
                settingsDataGrid.IsEnabled = false;
                ApplyButton.IsEnabled = false;
                CheckScreenCap.IsEnabled = false;

            }
        }

        private void CalibrateTracker(object sender, RoutedEventArgs e)
        {
            TrackerManager.CalibrateActiveTracker();
        }

        private void ShowReticle(object sender, RoutedEventArgs e)
        {
            if (reticleController == null)
                reticleController = new ReticleController();

            if (reticleController.IsShown())
            {
                reticleController.HideReticle();
                ActivateReticleButton.Content = Properties.Resources.ShowReticle;
            }
            else
            {
                reticleController.ShowReticle();
                ActivateReticleButton.Content = Properties.Resources.HideReticle;
            }
        }

        private void SessionSetupButton_Click(object sender, RoutedEventArgs e)
        {
            // Short circuit evaluation
            //  Before first use it will be null second time around it is unloaded
            if (sessionInformation == null || !sessionInformation.IsLoaded)
            {
                sessionInformation = new SessionSetupWindow();
                sessionInformation.Show();
            }
            if (sessionInformation.WindowState == WindowState.Minimized)
            {
                sessionInformation.WindowState = WindowState.Normal;
            }
            if (sessionInformation.IsLoaded)
            {
                sessionInformation.Activate();
            }
        }

        private void ShowEyeStatusWindow(object sender, RoutedEventArgs e)
        {
            TrackerManager.ShowEyeStatusWindow();
        }
    }
}
