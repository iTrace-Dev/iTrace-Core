using System;
using System.Windows;
using System.Configuration;
using System.Windows.Controls;
using System.Collections.Generic;

namespace iTrace_Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
            public string Option { get; set; }
            public string Value { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            TrackerManager = new TrackerManager();

            // Initialize Session
            SessionManager.GetInstance().SetScreenDimensions(System.Windows.SystemParameters.PrimaryScreenWidth,
                System.Windows.SystemParameters.PrimaryScreenHeight);

            // Default the session to the last used output directory and empty everything else
            SessionManager.GetInstance().SetupSession("", "", "", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            SessionManager.GetInstance().SetCalibration(new EmptyCalibrationResult());

            socketServer = new SocketServer();
            webSocketServer = new WebSocketServer();
            xmlGazeDataWriter = new XMLGazeDataWriter();

            InitializeSettingsGrid();
        }

        private void ApplicationLoaded(object sender, RoutedEventArgs e)
        {
            RefreshTrackerList();
        }

        private void InitializeSettingsGrid()
        {
            settings = new List<Setting>();
            List<string> options = new List<string> { "socket_port", "websocket_port"};

            foreach (string s in options)
            {
                settings.Add(new Setting { Option = s, Value = ConfigurationRegistry.Instance.AssignFromConfiguration(s, "") });
            }

            settingsDataGrid.ItemsSource = settings;
        }

        private void ApplySettings(object sender, RoutedEventArgs e)
        {
            foreach(Setting s in settings)
            {
                ConfigurationRegistry.Instance.WriteConfiguration(s.Option, s.Value);
            }
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

        private void MenuSettingsClick(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("SETTINGS!");
        }

        private void MenuExitClick(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("EXIT!");
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
				ActivateTrackerButton.Content = Properties.Resources.StartTracking;
                TrackerManager.StopTracker();
                if (CheckScreenCap.IsChecked.HasValue && CheckScreenCap.IsChecked.Value)
                {
                    rec.Dispose();
                }
                xmlGazeDataWriter.StopWriting();
                ActivateCalibrationButton.IsEnabled = true;
                ShowEyeStatusButton.IsEnabled = true;
            }
            else
            {
                //Start the session (SHOULD HAPPPEN FIRST)
                SessionManager.GetInstance().StartSession();

                // Load previously use directory for data storage or the current users desktop directory
                xmlGazeDataWriter.StartWriting(SessionManager.GetInstance().DataRootDir);

                socketServer.SendSessionData();
                webSocketServer.SendSessionData();

				//Name of .avi hardcoded for now
                if (CheckScreenCap.IsChecked.HasValue && CheckScreenCap.IsChecked.Value)
                {
                    rec = new Recorder(new RecorderParams(SessionManager.GetInstance().DataRootDir + "/screen_rec" + "-" + SessionManager.GetInstance().CurrentSessionTimeStamp + ".avi", 10, SharpAvi.KnownFourCCs.Codecs.MotionJpeg, 80)); //screenrecording start
                    SessionManager.GetInstance().GenerateScreenRecordingStart();
                }
				ActivateTrackerButton.Content = Properties.Resources.StopTracking;

                TrackerManager.StartTracker();
                ActivateCalibrationButton.IsEnabled = false;
                ShowEyeStatusButton.IsEnabled = false;
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

            if(reticleController.IsShown())
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
