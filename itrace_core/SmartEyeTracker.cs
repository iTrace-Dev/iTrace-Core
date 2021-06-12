using System;
using System.Net;
using System.Text;

namespace iTrace_Core
{
    class SmartEyeTracker : ITracker
    {
        private readonly String SMARTEYE_ADDRESS = "127.0.0.1"; //TODO option to select?
        private readonly int SMARTEYE_PORT = 4242; //TODO set to default from SE software
        private System.Net.Sockets.UdpClient Client;
        private IPEndPoint ep;
        private String TrackerName;
        private String TrackerSerialNumber;

        public SmartEyeTracker()
        {
            ep = new IPEndPoint(IPAddress.Parse(SMARTEYE_ADDRESS), SMARTEYE_PORT);

            TrackerName = "SmartEye Tracker";
            TrackerSerialNumber = "1234";

            try
            {
                Client = new System.Net.Sockets.UdpClient();
                Client.Connect(SMARTEYE_ADDRESS, SMARTEYE_PORT);

                //Set actual tracker name and serial

                TrackerInit();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine("SmartEye not connected!");
                Client = null;
            }
            catch (Exception e)
            {
                Console.WriteLine("SmartEye Connection Failed!");
                Console.WriteLine(e.ToString());
                Client = null;
            }
        }

        public bool TrackerFound()
        {
            //TODO: Because this is UDP, this does not actually mean the tracker is present and ready to go
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
            Console.WriteLine("START SE TRACKING");
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

        public void ShowEyeStatusWindow()
        {

        }

        private void TrackerInit()
        {

        }

        private void ListenForData()
        {
            while (true)
            {
                //Receive UDP packet
                //TODO listen for termination

                byte[] packet = Client.Receive(ref ep);

                Console.WriteLine("Packet received: {0}", Encoding.ASCII.GetString(packet, 0, packet.Length));
            }
        }
    }
}
