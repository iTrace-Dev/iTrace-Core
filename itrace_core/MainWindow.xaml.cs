using System;
using System.Windows;
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

        public MainWindow()
        {
            InitializeComponent();
            TrackerManager = new TrackerManager();
            Console.WriteLine("Screen Dimensions: {0}x{1}", System.Windows.SystemParameters.PrimaryScreenWidth, 
                System.Windows.SystemParameters.PrimaryScreenHeight);
        }

        private void ApplicationLoaded(object sender, RoutedEventArgs e)
        {
            RefreshTrackerList();
        }

        private void MenuSettingsClick(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("SETTINGS!");
        }

        private void MenuExitClick(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("EXIT!");
        }

        private void TrackerListChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TrackerList.SelectedIndex >= 0)
            {
                System.Console.WriteLine(TrackerList.SelectedItem.ToString());
                TrackerManager.SetActiveTracker(TrackerList.SelectedItem.ToString());
            }
        }

        private void RefreshAttachedTrackers(object sender, RoutedEventArgs e)
        {
            RefreshTrackerList();
        }

        private void RefreshTrackerList()
        {
            TrackerList.SelectedIndex = -1;
            TrackerManager.FindTrackers();
            TrackerList.ItemsSource = TrackerManager.GetAttachedTrackers();
        }

        private void StartTracker(object sender, RoutedEventArgs e)
        {
            if (TrackerManager.Running())
            {
				ActivateTrackerButton.Content = "Start Tracking";
                TrackerManager.StopTracker();
                if (CheckScreenCap.IsChecked.HasValue && CheckScreenCap.IsChecked.Value)
                {
                    rec.Dispose();
                }
            }
            else
            {
				//Name of .avi hardcoded for now
                if (CheckScreenCap.IsChecked.HasValue && CheckScreenCap.IsChecked.Value)
                {
                    rec = new Recorder(new RecorderParams("out.avi", 10, SharpAvi.KnownFourCCs.Codecs.MotionJpeg, 80)); //screenrecording start
                }
				ActivateTrackerButton.Content = "Stop Tracking";
                TrackerManager.StartTracker();
            }
        }

        private void CalibrateTracker(object sender, RoutedEventArgs e)
        {
            TrackerManager.CalibrateActiveTracker();
        }
    }
}
