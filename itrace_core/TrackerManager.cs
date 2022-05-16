/********************************************************************************************************************************************************
* @file TrackerManager.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

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
            //Ensure all trackers are cleaned up (Namely SmartEye must be fully disconnected)
            foreach (ITracker tracker in EyeTrackers)
            {
                //Consider making this common to ITracker
                if (tracker is SmartEyeTracker)
                    ((SmartEyeTracker)tracker).CleanupConnections();
            }

            EyeTrackers.Clear();
            EyeTrackers.Add(new MouseTracker());
            FindTobiiDevices();
            FindGazePointDevice();
            FindSmartEyeDevice();
        }

        private void FindSmartEyeDevice()
        {
            SmartEyeTracker seTracker = new SmartEyeTracker();
            if (seTracker.TrackerFound())
            {
                EyeTrackers.Add(seTracker);
            }
        }

        private void FindTobiiDevices()
        {
            //Sometimes the returned device will have a blank string DeviceName. I have no idea what causes this but it appears to be on Tobii's side, possible API problem.
            //AL

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
                    UpdateSessionInfo();
                    break;
                }
            }
        }

        private void UpdateSessionInfo()
        {
            SessionManager.GetInstance().SetTrackerData(ActiveTracker.GetTrackerName(), ActiveTracker.GetTrackerSerialNumber());
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

        public ITracker GetActiveTracker()
        {
            return ActiveTracker;
        }
    }
}
