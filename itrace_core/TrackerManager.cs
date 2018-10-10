using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    class TrackerManager
    {
        private List<ITracker> EyeTrackers;
        private ITracker ActiveTracker;
        private Boolean Tracking;

        public TrackerManager()
        {
            EyeTrackers = new List<ITracker>();
            FindTrackers();
            Tracking = false;
        }

        public void FindTrackers()
        {
            EyeTrackers.Clear();
            EyeTrackers.Add(new MouseTracker());
            FindTobiiDevices();
        }

        private void FindTobiiDevices()
        {
            Tobii.Research.EyeTrackerCollection eyeTrackers = Tobii.Research.EyeTrackingOperations.FindAllEyeTrackers();
            foreach (Tobii.Research.IEyeTracker eyeTracker in eyeTrackers)
            {
                EyeTrackers.Add(new TobiiTracker(eyeTracker));
            }
        }

        public void SetActiveTracker(String trackerName) {
            foreach (ITracker tracker in EyeTrackers)
            {
                if (tracker.GetTrackerName().Equals(trackerName))
                {
                    Console.WriteLine("FOUND AND SET ACTIVE TRACKER!");
                    ActiveTracker = tracker;
                    break;
                }
            }
        }

        public List<String> GetAttachedTrackers()
        {
            List<String> trackers = new List<string>();
            foreach (ITracker tracker in EyeTrackers)
            {
                trackers.Add(tracker.GetTrackerName());
            }

            return trackers;
        }

        public Boolean Running()
        {
            return Tracking;
        }

        public Boolean StartTracker()
        { 
            Tracking = true;
            ActiveTracker.StartTracker();
            return Tracking;
        }

        public Boolean StopTracker()
        {
            Tracking = false;
            ActiveTracker.StopTracker();
            return Tracking;
        }

        public void CalibrateActiveTracker()
        {
            if (ActiveTracker != null)
                ActiveTracker.EnterCalibration();
        }
    }
}
