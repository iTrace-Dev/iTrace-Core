using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    class GazePointTracker : ITracker
    {
        private readonly String GAZEPOINT_ADDRESS = "127.0.0.1";
        private readonly int ServerPort = 4242;
        private System.Net.Sockets.TcpClient Client;
        private System.IO.StreamReader Reader;
        private System.IO.StreamWriter Writer;
        private String TrackerName;

        public GazePointTracker()
        {
            try
            {
                TrackerName = "GP3";
                Client = new System.Net.Sockets.TcpClient();
                Client.Connect(GAZEPOINT_ADDRESS, ServerPort);
                Reader = new System.IO.StreamReader(Client.GetStream());
                Writer = new System.IO.StreamWriter(Client.GetStream());

                new System.Threading.Thread(() =>
                {
                    System.Threading.Thread.CurrentThread.IsBackground = true;
                    ListenForData();
                }).Start();

                TrackerInit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Client = null;
            }
        }

        public bool TrackerFound()
        {
            return (Client != null);
        }

        public String GetTrackerName()
        {
            return TrackerName;
        }

        public void StartTracker()
        {
            Console.WriteLine("START GP TRACKING");
            Writer.Write("<SET ID=\"ENABLE_SEND_DATA\" STATE=\"1\" />\r\n");
            Writer.Flush();
        }

        public void StopTracker()
        {
            Console.WriteLine("STOP GP TRACKING");
            Writer.Write("<SET ID=\"ENABLE_SEND_DATA\" STATE=\"0\" />\r\n");
            Writer.Flush();
        }

        public void EnterCalibration()
        {
            Console.WriteLine("ENTER CALIBRATION");
            Writer.Write("<SET ID=\"CALIBRATE_SHOW\" STATE=\"1\" />\r\n");
            Writer.Write("<SET ID=\"CALIBRATE_START\" STATE=\"1\" />\r\n");
            Writer.Flush();
        }

        public void LeaveCalibration()
        {
        }

        private void TrackerInit()
        {
            Console.WriteLine("INIT GAZEPOINT");
            Writer.Write("<SET ID=\"ENABLE_SEND_COUNTER\" STATE=\"1\" />\r\n");
            Writer.Write("<SET ID=\"ENABLE_SEND_TIME\" STATE=\"1\" />\r\n");
            Writer.Write("<SET ID=\"ENABLE_SEND_TIME_TICK\" STATE=\"1\" />\r\n");
            Writer.Write("<SET ID=\"ENABLE_SEND_POG_FIX\" STATE=\"1\" />\r\n");
            Writer.Write("<SET ID=\"ENABLE_SEND_POG_LEFT\" STATE=\"1\" />\r\n");
            Writer.Write("<SET ID=\"ENABLE_SEND_POG_RIGHT\" STATE=\"1\" />\r\n");
            Writer.Write("<SET ID=\"ENABLE_SEND_POG_BEST\" STATE=\"1\" />\r\n");
            Writer.Write("<SET ID=\"ENABLE_SEND_PUPIL_LEFT\" STATE=\"1\" />\r\n");
            Writer.Write("<SET ID=\"ENABLE_SEND_PUPIL_RIGHT\" STATE=\"1\" />\r\n");
            Writer.Write("<SET ID=\"ENABLE_SEND_EYE_LEFT\" STATE=\"1\" />\r\n");
            Writer.Write("<SET ID=\"ENABLE_SEND_EYE_RIGHT\" STATE=\"1\" />\r\n");
            Writer.Write("<SET ID=\"ENABLE_SEND_CURSOR\" STATE=\"1\" />\r\n");
            Writer.Write("<GET ID=\"PRODUCT_ID\" />\r\n");
            Writer.Flush();
        }

        private void ListenForData()
        {
            Console.WriteLine("LISTEN!");
            String gazeData = "";
            while (!gazeData.Contains("<ACK ID=\"ENABLE_SEND_DATA\" STATE=\"0\" />"))
            {
                gazeData = Reader.ReadLine();
                Console.WriteLine(gazeData);
            }
        }
    }
}
