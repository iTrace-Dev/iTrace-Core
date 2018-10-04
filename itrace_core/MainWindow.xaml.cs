using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace iTrace_Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TrackerManager TrackerManager;

        public MainWindow()
        {
            InitializeComponent();
            TrackerManager = new TrackerManager();
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
            }
            else
            {
                ActivateTrackerButton.Content = "Stop Tracking";
                TrackerManager.StartTracker();
            }
        }
    }
}
