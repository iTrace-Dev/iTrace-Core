using System;
using System.Windows;
using System.Configuration;
using System.Windows.Controls;

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

        public MainWindow()
        {
            InitializeComponent();
            TrackerManager = new TrackerManager();
            Console.WriteLine("Screen Dimensions: {0}x{1}", System.Windows.SystemParameters.PrimaryScreenWidth, 
                System.Windows.SystemParameters.PrimaryScreenHeight);

            socketServer = new SocketServer();
            webSocketServer = new WebSocketServer();
            xmlGazeDataWriter = new XMLGazeDataWriter();
        }

        private void ApplicationLoaded(object sender, RoutedEventArgs e)
        {
            RefreshTrackerList();
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
        }

        private void MenuSettingsClick(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("SETTINGS!");
        }

        private void MenuExitClick(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("EXIT!");

            if(xmlGazeDataWriter.Writing)
            {
                xmlGazeDataWriter.StopWriting();
            }

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
                xmlGazeDataWriter.StartWriting(ConfigurationRegistry.Instance.AssignFromConfiguration("xml_output_filename", "out.xml"));
                socketServer.SendSessionData(SessionManager.GetInstance());
                webSocketServer.SendSessionData(SessionManager.GetInstance());

				//Name of .avi hardcoded for now
                if (CheckScreenCap.IsChecked.HasValue && CheckScreenCap.IsChecked.Value)
                {
                    rec = new Recorder(new RecorderParams("out.avi", 10, SharpAvi.KnownFourCCs.Codecs.MotionJpeg, 80)); //screenrecording start
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
