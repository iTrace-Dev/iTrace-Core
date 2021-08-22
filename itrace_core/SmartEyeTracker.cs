using System;
using System.Net;
using System.Text;

namespace iTrace_Core
{
    class SmartEyeTracker : ITracker
    {
        private readonly String SMARTEYE_ADDRESS = "192.169.100.45"; //Should be determined automaticall when openDataStreamUDP rpc call is made
        private readonly String SMARTEYE_SERVER = "192.169.100.42"; //Remote address, computer running SmartEye
        private readonly int SMARTEYE_PORT_REALTIME = 5800; //default from SE software
        private readonly int SMARTEYE_PORT_LATENT = 5799; //TODO set to default from SE software
        private readonly int SMARTEYE_PORT_RPC = 8100; //default from SE software

        private System.Net.Sockets.UdpClient RealtimeClient;
        private System.Net.Sockets.TcpClient LatentClient;
        private System.Net.Sockets.TcpClient RpcClient;
        private IPEndPoint realtimeEndpoint;    //For real time data
        private IPEndPoint latentEndpoint;      //For processed/filtered data
        private IPEndPoint rpcEndpoint;      //For sending json RPC commands to SmartEye
        private String TrackerName;
        private String TrackerSerialNumber;

        private bool Listen;

        public SmartEyeTracker()
        {
            IPAddress address = IPAddress.Parse(SMARTEYE_ADDRESS);
            IPAddress serverAddress = IPAddress.Parse(SMARTEYE_SERVER);
            realtimeEndpoint = new IPEndPoint(address, SMARTEYE_PORT_REALTIME);
            latentEndpoint = new IPEndPoint(serverAddress, SMARTEYE_PORT_LATENT); 
            rpcEndpoint = new IPEndPoint(serverAddress, SMARTEYE_PORT_RPC);

            TrackerName = "SmartEye Tracker";
            TrackerSerialNumber = "1234";

            try
            {
                RealtimeClient = new System.Net.Sockets.UdpClient(realtimeEndpoint);
                LatentClient = new System.Net.Sockets.TcpClient();
                LatentClient.Connect(latentEndpoint);

                RpcClient = new System.Net.Sockets.TcpClient();
                RpcClient.Connect(rpcEndpoint);

                //TODO: Set actual tracker name and serial

                //TODO: use verbatim strings, or preferrably a JSON builder
                SendRpc(MakeSENetstring("{\"jsonrpc\":\"2.0\", \"method\":\"getState\", \"id\":0}"));

                byte[] recvBuffer = new byte[RpcClient.ReceiveBufferSize];
                System.Net.Sockets.NetworkStream recvStream = RpcClient.GetStream();
                recvStream.Read(recvBuffer, 0, RpcClient.ReceiveBufferSize);

                String response = Encoding.UTF8.GetString(recvBuffer);
                response = TrimSENetstring(response.TrimEnd('\0'));

                Console.WriteLine("JSON response: {0}\n", response);

                TrackerInit();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine("SmartEye not connected: " + e);
                RealtimeClient = null;
            }
            catch (Exception e)
            {
                Console.WriteLine("SmartEye Connection Failed: " + e);
                Console.WriteLine(e.ToString());
                RealtimeClient = null;
            }
        }

        public bool TrackerFound()
        {
            //TODO: Because this is UDP, this does not actually mean the tracker is present and ready to go
            //Checking presence via RPC might be excessive?

            if (RealtimeClient != null)
            {
                Console.WriteLine("Client connected");

                if (LatentClient == null)
                    Console.WriteLine("No latent data client!");

                if (RpcClient == null)
                    Console.WriteLine("No RPC connection!");

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

        //Converts raw rpc command into a netstring as defined in the Programmers Guide
        public static String MakeSENetstring(String rpc)
        {
            return rpc.Length.ToString() + ":" + rpc + ",";
        }

        //Converts a netstring to a json string
        //A netstring may look like this 16:{"memes":"dank"},
        //Where 16 is the length of the json before it was formatted to a netstring
        public static String TrimSENetstring(String netstring)
        {
            int length = 0;
            String json = null;

            for (int i = 0; i < netstring.Length; i++)
            {
                //Find end of length prefix
                if ( !(netstring[i] >= '0' && netstring[i] <= '9'))
                {
                    //Parse length prefix
                    length = int.Parse(netstring.Substring(0, i));

                    //Trim off length prefix
                    json = netstring.Substring(i + 1);
                    break;
                }
            }

            if (json == null || !json.EndsWith(","))
                throw new Exception("Tried to parse invalid netstring: " + netstring);

            //Trim off trailing comma
            json = json.Substring(0, json.Length - 1);

            //Check length
            if (json.Length != length)
                throw new Exception("Netstring body does not match length prefix: " + netstring);

            return json;
        }

        //Send RPC netstring using TCP
        public void SendRpc(String netstring)
        {
            //Unsure if this econding is right
            byte[] buf = Encoding.UTF8.GetBytes(netstring);
            RpcClient.GetStream().Write(buf, 0, buf.Length);
        }

        public void StartTracker()
        {
            String startTrackingJson = MakeSENetstring("{\"jsonrpc\":\"2.0\", \"method\":\"startTracking\", \"id\":0}");
            SendRpc(startTrackingJson);

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
            String stopTrackingJson = MakeSENetstring("{\"jsonrpc\":\"2.0\", \"method\":\"stopTracking\", \"id\":0}");
            SendRpc(stopTrackingJson);

            Listen = false;
        }

        public void EnterCalibration()
        {
            //Open calibration window (not working yet)
            String enterCalibrationJson = MakeSENetstring("{\"jsonrpc\":\"2.0\", \"method\":\"calibrateGaze\", \"id\":0}");
            SendRpc(enterCalibrationJson);

            //TODO: receive and store response
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
                //byte[] packet = LatentClient.Receive(ref latentEndpoint);

                //SmartEyeGazeData gaze = new SmartEyeGazeData(packet);

                //TODO: store fixations and blinks somewhere
            }
        }
    }
}
