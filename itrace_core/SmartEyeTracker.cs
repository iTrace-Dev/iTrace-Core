using System;
using System.Net;
using System.Text;

namespace iTrace_Core
{
    class SmartEyeTracker : ITracker
    {
        private readonly String SMARTEYE_ADDRESS = "192.168.100.45"; //TODO option to select?
        private readonly int SMARTEYE_PORT_REALTIME = 5800; //TODO set to default from SE software
        private readonly int SMARTEYE_PORT_LATENT = 5801; //TODO set to default from SE software
        private System.Net.Sockets.UdpClient RealtimeClient;
        private System.Net.Sockets.UdpClient LatentClient;
        private IPEndPoint realtimeEndpoint;    //For real time data
        private IPEndPoint latentEndpoint;      //For processed/filtered data
        private String TrackerName;
        private String TrackerSerialNumber;

        private bool Listen;

        public SmartEyeTracker()
        {
            realtimeEndpoint = new IPEndPoint(IPAddress.Parse(SMARTEYE_ADDRESS), SMARTEYE_PORT_REALTIME);
            latentEndpoint = new IPEndPoint(IPAddress.Parse(SMARTEYE_ADDRESS), SMARTEYE_PORT_LATENT);

            TrackerName = "SmartEye Tracker";
            TrackerSerialNumber = "1234";

            try
            {
                RealtimeClient = new System.Net.Sockets.UdpClient(realtimeEndpoint);
                LatentClient = new System.Net.Sockets.UdpClient(latentEndpoint);

                //TODO: Set actual tracker name and serial

                TrackerInit();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine("SmartEye not connected!");
                RealtimeClient = null;
            }
            catch (Exception e)
            {
                Console.WriteLine("SmartEye Connection Failed!");
                Console.WriteLine(e.ToString());
                RealtimeClient = null;
            }
        }

        public bool TrackerFound()
        {
            //TODO: Because this is UDP, this does not actually mean the tracker is present and ready to go
            if (RealtimeClient != null)
            {
                Console.WriteLine("Client connected");

                if (LatentClient == null)
                    Console.WriteLine("No latent data client!");

                return true;
            }
            else { return false; }
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

            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.CurrentThread.IsBackground = true;
                ListenForLatent();
            }).Start();

            Console.WriteLine("START SE TRACKING");
        }

        public void StopTracker()
        {
            Listen = false;
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
            Listen = true;

            while (Listen)
            {
                //Receive UDP packet
                //TODO listen for termination

                byte[] packet = RealtimeClient.Receive(ref realtimeEndpoint);

                SmartEyeGazeData gaze = new SmartEyeGazeData(packet);
                GazeHandler.Instance.EnqueueGaze(gaze);
            }
        }

        private void ListenForLatent()
        {
            while (Listen)
            {
                byte[] packet = LatentClient.Receive(ref latentEndpoint);

                SmartEyeGazeData gaze = new SmartEyeGazeData(packet);

                //TODO: store fixations and blinks somewhere
            }
        }
    }
}
