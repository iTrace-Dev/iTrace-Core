using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    class SmartEyeTracker : ITracker
    {
        private readonly String SMARTEYE_ADDRESS = "127.0.0.1";
        private readonly int SMARTEYE_PORT = 9001; //TODO determine the correct port
        private System.Net.Sockets.UdpClient Client;
        private System.IO.StreamReader Reader;
        private System.IO.StreamWriter Writer;
        private String TrackerName;
        private String TrackerSerialNumber;

        public void EnterCalibration()
        {
            throw new NotImplementedException();
        }

        public bool TrackerFound()
        {
            return (Client != null);
        }

        public String GetTrackerName()
        {
            return TrackerName;
        }

        public String GetTrackerSerialNumber()
        {
            return TrackerSerialNumber;
        }

        public void LeaveCalibration()
        {
            throw new NotImplementedException();
        }

        public void ShowEyeStatusWindow()
        {
            throw new NotImplementedException();
        }

        public void StartTracker()
        {
            throw new NotImplementedException();
        }

        public void StopTracker()
        {
            throw new NotImplementedException();
        }
    }
}
