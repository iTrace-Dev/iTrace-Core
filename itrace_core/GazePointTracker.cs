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
        private readonly int GAZEPOINT_PORT = 4242;
        private System.Net.Sockets.TcpClient Client;
        private System.IO.StreamReader Reader;
        private System.IO.StreamWriter Writer;
        private String TrackerName;
        private String TrackerSerialNumber;

        public GazePointTracker()
        {
            try
            {
                Client = new System.Net.Sockets.TcpClient();
                Client.Connect(GAZEPOINT_ADDRESS, GAZEPOINT_PORT);
                Reader = new System.IO.StreamReader(Client.GetStream());
                Writer = new System.IO.StreamWriter(Client.GetStream());

                Writer.Write("<GET ID=\"PRODUCT_ID\" />\r\n"); Writer.Flush();
                System.Xml.XmlDocument gazePointData = new System.Xml.XmlDocument();
                gazePointData.LoadXml(Reader.ReadLine());
                TrackerName = gazePointData.DocumentElement.GetAttribute("VALUE");

                Writer.Write("<GET ID=\"SERIAL_ID\" />\r\n"); Writer.Flush();
                gazePointData.LoadXml(Reader.ReadLine());
                TrackerSerialNumber = gazePointData.DocumentElement.GetAttribute("VALUE");

                TrackerInit();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine("Gaze Point not connected!");
                Client = null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Gaze Point Connection Failed!");
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

        public String GetTrackerSerialNumber()
        {
            return TrackerSerialNumber;
        }

        public void StartTracker()
        {
            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.CurrentThread.IsBackground = true;
                ListenForData();
            }).Start();
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
            Writer.Write("<SET ID=\"CALIBRATE_SHOW\" STATE=\"1\" />\r\n");Writer.Flush();Reader.ReadLine();
            Writer.Write("<SET ID=\"CALIBRATE_START\" STATE=\"1\" />\r\n"); Writer.Flush();Reader.ReadLine();

            String calibrationData = "";
            int calibrationPointCount = 0;
            while (!calibrationData.Contains("ID=\"CALIB_RESULT\""))
            {
                calibrationData = Reader.ReadLine();
                ++calibrationPointCount;
            }

            // Get complete calibration data
            SessionManager.GetInstance().SetCalibration(new GazePointCalibrationResult(calibrationData, ((calibrationPointCount - 1) / 2)));
            Console.WriteLine(calibrationData + "\r\nCAL POINTS:" + (calibrationPointCount - 1) / 2);
        }

        public void LeaveCalibration() {}

        public void ShowEyeStatusWindow()
        {
            Writer.Write("<SET ID=\"TRACKER_DISPLAY\" STATE=\"1\" />\r\n");
            Writer.Flush();
            Console.WriteLine(Reader.ReadLine());
        }

        private void TrackerInit()
        {
            Console.WriteLine("INIT GAZEPOINT");
            
            // Set gazepoint to default to the primary screen
            Writer.Write("<SET ID=\"SCREEN_SIZE\" X=\"0\" Y=\"0\" WIDTH=\"{0}\" HEIGHT=\"{1}\" />\r\n", System.Windows.SystemParameters.PrimaryScreenWidth, System.Windows.SystemParameters.PrimaryScreenHeight); Writer.Flush(); Console.WriteLine(Reader.ReadLine());

            // Enable timing and counting for messages (recorded data and debuging)
            Writer.Write("<SET ID=\"ENABLE_SEND_COUNTER\" STATE=\"1\" />\r\n"); Writer.Flush(); Console.WriteLine(Reader.ReadLine());
            Writer.Write("<SET ID=\"ENABLE_SEND_TIME_TICK\" STATE=\"1\" />\r\n"); Writer.Flush(); Console.WriteLine(Reader.ReadLine());

            // Enable tracking Left and Right gazes
            Writer.Write("<SET ID=\"ENABLE_SEND_POG_LEFT\" STATE=\"1\" />\r\n"); Writer.Flush(); Console.WriteLine(Reader.ReadLine());
            Writer.Write("<SET ID=\"ENABLE_SEND_POG_RIGHT\" STATE=\"1\" />\r\n"); Writer.Flush(); Console.WriteLine(Reader.ReadLine());

            // Enable tracking the best X Y gaze based on an average of Left and Right eyes
            Writer.Write("<SET ID=\"ENABLE_SEND_POG_BEST\" STATE=\"1\" />\r\n"); Writer.Flush(); Console.WriteLine(Reader.ReadLine());

            // Enable pupil data for each eye
            Writer.Write("<SET ID=\"ENABLE_SEND_PUPIL_LEFT\" STATE=\"1\" />\r\n"); Writer.Flush(); Console.WriteLine(Reader.ReadLine());
            Writer.Write("<SET ID=\"ENABLE_SEND_PUPIL_RIGHT\" STATE=\"1\" />\r\n"); Writer.Flush(); Console.WriteLine(Reader.ReadLine());

            // Enable user space data for each eye (X, Y, Z)
            Writer.Write("<SET ID=\"ENABLE_SEND_EYE_LEFT\" STATE=\"1\" />\r\n"); Writer.Flush(); Console.WriteLine(Reader.ReadLine());
            Writer.Write("<SET ID=\"ENABLE_SEND_EYE_RIGHT\" STATE=\"1\" />\r\n"); Writer.Flush(); Console.WriteLine(Reader.ReadLine());

        }

        private void ListenForData()
        {
            String gazeData = "";
            while (!gazeData.Contains("<ACK ID=\"ENABLE_SEND_DATA\" STATE=\"0\" />"))
            {
                gazeData = Reader.ReadLine();
                if (gazeData.Contains("<REC"))
                {
                    GazeHandler.Instance.EnqueueGaze(new GazepointGazeData(gazeData));
                }
            }
        }
    }
}
