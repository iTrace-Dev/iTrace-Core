using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    class GazePointTracker : ITracker
    {
        private readonly System.Net.IPAddress GAZEPOINT_ADDRESS = System.Net.IPAddress.Parse("127.0.0.1");
        private readonly int ServerPort = 4242;

        GazePointTracker()
        {

        }

        public String GetTrackerName()
        {
            return "Foobar";
        }

        public void StartTracker()
        {
        }

        public void StopTracker()
        {
        }

        public void EnterCalibration()
        {
        }

        public void LeaveCalibration()
        {
        }
    }
}
