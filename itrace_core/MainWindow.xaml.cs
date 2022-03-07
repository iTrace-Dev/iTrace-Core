using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using iTrace_Core.Properties;
//
namespace iTrace_Core
{
    enum ReplayType
    {
        Fixed,
        Proportional,
        Bidirectional
    }
    public partial class MainWindow : Window
    {
        TrackerManager TrackerManager;
        Recorder rec;
        ReticleController reticleController;
        SocketServer socketServer = SocketServer.Instance();
        WebSocketServer webSocketServer = WebSocketServer.Instance();
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

        // DejaVu
        EventRecorder eventRecorder;
        EventReplayer eventReplayer;
        ReplayType replayType = ReplayType.Fixed;
        private WindowPositionManager windowPositionManager;

        public MainWindow()
        {
            InitializeComponent();
            TrackerManager = new TrackerManager();

            // DejaVu
            windowPositionManager = new WindowPositionManager();

            // Initialize Session
            SessionManager.GetInstance().SetScreenDimensions(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);

            // Default the session to the last used output directory and empty everything else
            SessionManager.GetInstance().SetupSession("", "", "", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            SessionManager.GetInstance().ClearCalibration();

            xmlGazeDataWriter = new XMLGazeDataWriter();

            DataOutputDir.Text = SessionManager.GetInstance().DataRootDir;

            InitializeSettingsGrid();
        }
        private void StopWindowPositionManager(object sender, EventArgs e)
        {
            Console.WriteLine("StopWindowManager");
            windowPositionManager.Stop();
        }
        private void RestoreWindowState(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.WindowState = WindowState.Normal;
            });
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Console.WriteLine("Window Closing");
            if (eventRecorder != null && eventRecorder.IsRecordInProgress) eventRecorder.Dispose();
            if (eventReplayer != null && eventReplayer.IsReplayInProgress) eventReplayer.StopReplay();
            windowPositionManager.Stop();
        }
        private void ApplicationLoaded(object sender, RoutedEventArgs e)
        {
            RefreshTrackerList();

            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();
        }
        private void InitializeSettingsGrid()
        {
            settings = new List<Setting>()
            {
                new Setting("socket_port") { Value = Settings.Default.socket_port.ToString() },
                new Setting("websocket_port") { Value = Settings.Default.websocket_port.ToString() },
                new Setting("smarteye_ip_address") { Value = Settings.Default.smarteye_ip_address.ToString() },
                new Setting("smarteye_ip_port") { Value = Settings.Default.smarteye_ip_port.ToString() },
                new Setting("calibration_monitor") { Value = Settings.Default.calibration_monitor.ToString() }
            };

            settingsDataGrid.ItemsSource = settings;
        }


        private void ApplySettings(object sender, RoutedEventArgs e)
        {
            int socketPort = 0;
            int websocketPort = 0;
            int calibrationMonitor = 0;
            if (!(int.TryParse(settings[0].Value, out socketPort) && int.TryParse(settings[1].Value, out websocketPort) && int.TryParse(settings[2].Value, out calibrationMonitor)))
            {
                MessageBox.Show(Properties.Resources.PortValuesMustBeNumeric, Properties.Resources.InvalidPortValue, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (socketPort == websocketPort)
            {
                MessageBox.Show(Properties.Resources.PortValuesCannotBeSameValue, Properties.Resources.DuplicatePortValues, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // These are separate in the event that the max values are actually different
            if (!((socketPort <= SocketServer.MAX_PORT_NUM) && (socketPort >= SocketServer.MIN_PORT_NUM)))
            {
                MessageBox.Show(Properties.Resources.SocketValuesMustBeInRange + SocketServer.MIN_PORT_NUM + "-" + SocketServer.MAX_PORT_NUM + "!", Properties.Resources.InvalidSocketPortValue, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!((websocketPort <= WebSocketServer.MAX_WEBSOCKET_PORT_NUM) && (websocketPort >= WebSocketServer.MIN_WEBSOCKET_PORT_NUM)))
            {
                MessageBox.Show(Properties.Resources.WebSocketValuesMustBeInRange + WebSocketServer.MIN_WEBSOCKET_PORT_NUM + "-" + WebSocketServer.MAX_WEBSOCKET_PORT_NUM + "!", Properties.Resources.InvalidWebSocketPortValue, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!((calibrationMonitor <= System.Windows.Forms.Screen.AllScreens.Length) && (calibrationMonitor > 0)))
            {
                MessageBox.Show(Properties.Resources.MonitorIndexOutOfRange, Properties.Resources.MonitorIndexIncorrectValue, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Settings.Default.socket_port = Convert.ToInt32(settings[0].Value);
            Settings.Default.websocket_port = Convert.ToInt32(settings[1].Value);
            Settings.Default.calibration_monitor = Convert.ToInt32(settings[2].Value);
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
        //////////////////////////////
        /// Session Setup Tab
        //////////////////////////////
        private void DirectoryBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDialogue = new System.Windows.Forms.FolderBrowserDialog();
            folderDialogue.SelectedPath = SessionManager.GetInstance().DataRootDir;

            System.Windows.Forms.DialogResult result = folderDialogue.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialogue.SelectedPath))
            {
                DataOutputDir.Text = folderDialogue.SelectedPath;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.GetInstance().SetupSession(TaskName.Text, ResearcherName.Text, ParticipantID.Text, DataOutputDir.Text);
            //Close();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            TaskName.Text = "";
            ResearcherName.Text = "";
            ParticipantID.Text = "";
            DataOutputDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            SessionManager.GetInstance().SetupSession(TaskName.Text, ResearcherName.Text, ParticipantID.Text, DataOutputDir.Text);
        }

        //////////////////////////////
        /// iTrace Tracking Tab
        //////////////////////////////

        private void TrackerListChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TrackerList.SelectedIndex >= 0)
            {
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
                settingsDataGrid.IsEnabled = true;
                ApplyButton.IsEnabled = true;
                CheckScreenCap.IsEnabled = true;

                if (CheckDejavuRecord.IsChecked.HasValue && CheckDejavuRecord.IsChecked.Value)
                {
                    windowPositionManager.Stop();
                    eventRecorder.StopRecording();
                    eventRecorder.Dispose();
                }
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
                //SessionSetupButton.IsEnabled = false;
                settingsDataGrid.IsEnabled = false;
                ApplyButton.IsEnabled = false;
                CheckScreenCap.IsEnabled = false;

                // DejaVu Record
                if (CheckDejavuRecord.IsChecked.HasValue && CheckDejavuRecord.IsChecked.Value)
                {
                    eventRecorder = new EventRecorder(new ComputerEventWriter(SessionManager.GetInstance().DataRootDir + "\\out.csv"));
                    windowPositionManager.Start();
                    eventRecorder.ConnectToCore();
                    eventRecorder.StartRecording();
                }
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

        //////////////////////////////
        /// DejaVu Replay Tab
        //////////////////////////////
        private void ReplayButtonClicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.InitialDirectory = SessionManager.GetInstance().DataRootDir;
            fileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            fileDialog.FilterIndex = 2;
            fileDialog.RestoreDirectory = true;

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = fileDialog.FileName;

                Console.WriteLine(replayType);
                int option = Int32.Parse(ReplayOption.Text);
                switch (replayType)
                {
                    case ReplayType.Fixed:
                        eventReplayer = new FixedPauseEventReplayer(path, option);
                        break;
                    case ReplayType.Proportional:
                        eventReplayer = new ProportionalEventReplayer(path, option);
                        break;
                    case ReplayType.Bidirectional:
                        eventReplayer = new BidirectionalCommunicationEventReplayer(path, option);
                        break;
                }

                eventReplayer.OnReplayFinished += StopWindowPositionManager;
                eventReplayer.OnReplayFinished += RestoreWindowState;

                windowPositionManager.Start();
                SocketServer.Instance().ReplayAcceptQueuedClients();
                eventReplayer.StartReplay();

                // TODO: Disable buttons, minimize window
                this.WindowState = (WindowState)System.Windows.Forms.FormWindowState.Minimized;
            }
        }

        private void FixedPauseChecked(object sender, RoutedEventArgs e)
        {
            replayType = ReplayType.Fixed;
            OptionLabel.Content = "Pause Length (ms)";
            ReplayOption.Text = "10";
            OptionLabel.Visibility = Visibility.Visible;
            //OptionHeader.Visibility = Visibility.Visible;
            ReplayOption.Show();
        }

        private void ProportionalPauseChecked(object sender, RoutedEventArgs e)
        {
            replayType = ReplayType.Proportional;
            OptionLabel.Content = "Scale Factor";
            ReplayOption.Text = "3";
            OptionLabel.Visibility = Visibility.Visible;
            //OptionHeader.Visibility = Visibility.Visible;
            ReplayOption.Show();
        }

        private void BidirectionalPauseChecked(object sender, RoutedEventArgs e)
        {
            replayType = ReplayType.Bidirectional;
            OptionLabel.Visibility = Visibility.Hidden;
            //OptionHeader.Visibility = Visibility.Hidden;
            ReplayOption.Hide();

        }
    }
}