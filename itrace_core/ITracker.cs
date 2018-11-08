using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    interface ITracker
    {
        void EnterCalibration();
        void LeaveCalibration();
        void StartTracker();
        void StopTracker();
        String GetTrackerName();
    }
}
