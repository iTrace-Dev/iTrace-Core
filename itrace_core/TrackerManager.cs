using System;
using System.Collections.Generic;

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
            Tracking = false;
        }

        public void FindTrackers()
        {
            EyeTrackers.Clear();
            EyeTrackers.Add(new MouseTracker());
            FindTobiiDevices();
            FindGazePointDevice();
        }

        private void FindTobiiDevices()
        {
            Tobii.Research.EyeTrackerCollection eyeTrackers = Tobii.Research.EyeTrackingOperations.FindAllEyeTrackers();
            foreach (Tobii.Research.IEyeTracker eyeTracker in eyeTrackers)
            {
                EyeTrackers.Add(new TobiiTracker(eyeTracker));
            }
        }

        private void FindGazePointDevice()
        {
            GazePointTracker gp = new GazePointTracker();
            if (gp.TrackerFound())
            {
                EyeTrackers.Add(gp);
            }
        }

        public void SetActiveTracker(String trackerName)
        {
            foreach (ITracker tracker in EyeTrackers)
            {
                if (tracker.GetTrackerName().Equals(trackerName))
                {
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
            GazeHandler.Instance.StartHandler();
            ActiveTracker.StartTracker();
            return Tracking;
        }

        public Boolean StopTracker()
        {
            Tracking = false;

            if (ActiveTracker != null)
            {
                GazeData empty = null;
                GazeHandler.Instance.EnqueueGaze(empty);
                ActiveTracker.StopTracker();
            }

            return Tracking;
        }

        public void CalibrateActiveTracker()
        {
            if (ActiveTracker != null)
                ActiveTracker.EnterCalibration();
        }

        public void ShowEyeStatusWindow()
        {
            if (ActiveTracker != null)
                ActiveTracker.ShowEyeStatusWindow();
        }
    }
}
