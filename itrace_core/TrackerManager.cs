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
        private String activeTracker;
        public string ActiveTracker { get => activeTracker; set => activeTracker = value; }

        public TrackerManager()
        {
            EyeTrackers = new List<ITracker>();
            FindTrackers();
        }

        public void FindTrackers()
        {
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

        public List<String> GetAttachedTrackers()
        {
            List<String> trackers = new List<string>();
            foreach (ITracker tracker in EyeTrackers)
            {
                trackers.Add(tracker.GetTrackerName());
            }

            return trackers;
        }
    }
}
