using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    class MouseTracker: ITracker
    {
        private readonly String trackerName;

        public MouseTracker()
        {
            trackerName = "Mouse";
        }

        public String GetTrackerName()
        {
            return trackerName;
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
