using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    class TobiiTracker: ITracker
    {
        private readonly Tobii.Research.IEyeTracker TrackingDevice;

        public TobiiTracker(Tobii.Research.IEyeTracker foundDevice)
        {
            TrackingDevice = foundDevice;
        }

        public String GetTrackerName()
        {
            return TrackingDevice.DeviceName;
        }

        public void StartTracker()
        {
            //TODO
        }

        public void StopTracker()
        {
            //TODO
        }

        public void EnterCalibration()
        {
            //TODO
        }

        public void LeaveCalibration()
        {
            //TODO
        }
    }
}
