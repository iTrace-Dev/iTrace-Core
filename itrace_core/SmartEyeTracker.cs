using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using iTrace_Core.Properties;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace iTrace_Core
{
    class SmartEyeTracker : ITracker
    {
        private readonly int SMARTEYE_PORT_LATENT = 5799; //TODO set to default from SE software
        private readonly int SMARTEYE_PORT_RPC = 8100; //This is the default from SE software

        private System.Net.Sockets.UdpClient RealtimeClient;
        private System.Net.Sockets.TcpClient LatentClient;
        private System.Net.Sockets.TcpClient RpcClient;
        private IPEndPoint realtimeEndpoint;    //For real time data
        private IPEndPoint latentEndpoint;      //For processed/filtered data
        private IPEndPoint rpcEndpoint;      //For sending json RPC commands to SmartEye
        private String TrackerName;
        private String TrackerSerialNumber;
        private String WorldModelString;

        private byte[] recvBuffer;

        private bool Listen;

        public SmartEyeTracker()
        {
            TrackerName = "SmartEye Tracker";
            TrackerSerialNumber = "Unknown"; //SE does not report a serial, make up some kind of hash?

            //TODO catch parse exception?
            IPAddress rpcAddress = IPAddress.Parse(Settings.Default.smarteye_ip_address);
            rpcEndpoint = new IPEndPoint(rpcAddress, SMARTEYE_PORT_RPC);
            
            try
            {
                //Try to connect to the RPC server on SmartEye host machine
                RpcClient = new System.Net.Sockets.TcpClient();
                RpcClient.Connect(rpcEndpoint);

                recvBuffer = new byte[RpcClient.ReceiveBufferSize];
                System.Net.Sockets.NetworkStream recvStream = RpcClient.GetStream();

                SendRpc(new SERPC("getState").GetNetstring());
                ReceiveRpcResponse(); //Dummy

                //Get actual tracker name

                SendRpc(new SERPC("getProductName").GetNetstring());
                JToken prod = ReceiveRpcResponse().GetValue("result");
                TrackerName = prod.Value<string>();

                //Shouldn't do this here?
                
                //Retrieve calibration from SE (stdDev and accuracy)
                SendRpc(new SERPC("retrieveCalibrationResults").GetNetstring());
                JToken cal = ReceiveRpcResponse().GetValue("result");

                JToken stdDev = cal.SelectToken("stdDev");
                double stdLeft = stdDev.Value<JToken>(0).Value<double>();
                double stdRight = stdDev.Value<JToken>(1).Value<double>();

                JToken accuracy = cal.SelectToken("accuracy");
                double accLeft = accuracy.Value<JToken>(0).Value<double>();
                double accRight = accuracy.Value<JToken>(1).Value<double>();

                //Get world and calibration points
                GetWorldCalibration();
            }
            catch (Exception e)
            {
                Console.WriteLine("SmartEye Connection Failed, could not connect to RPC server" +
                    " on " + rpcEndpoint.ToString() + " : " + e);

                RpcClient = null;
                return;
            }

            try
            {
                IPAddress selfAddress = null;
                IPHostEntry hostname = Dns.GetHostEntry(Dns.GetHostName());

                //Search our IP addresses for one on the same network as the smarteye server
                foreach (IPAddress ip in hostname.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        Console.WriteLine(ip.ToString());

                        //Warning: this assumes a class C address for now
                        byte[] ipAddr = ip.GetAddressBytes();
                        byte[] serverAddr = rpcAddress.GetAddressBytes();

                        //Check if first three bytes match
                        //TODO: make this work for other address classes
                        if (ipAddr[0] == serverAddr[0] && ipAddr[1] == serverAddr[1] && ipAddr[2] == serverAddr[2])
                        {
                            Console.WriteLine("Our ip is: " + ip.ToString());
                            selfAddress = ip;
                            break;
                        }
                    }
                }

                if (selfAddress == null)
                {
                    Console.WriteLine("Could not determine host IP");
                    RealtimeClient = null;
                    return;
                }

                realtimeEndpoint = new IPEndPoint(selfAddress, Settings.Default.smarteye_ip_port);
                RealtimeClient = new System.Net.Sockets.UdpClient(realtimeEndpoint);
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't open realtime data port (UDP) for SmartEye: " + e);
                RealtimeClient = null;
                return;
            }

            try
            {
                latentEndpoint = new IPEndPoint(IPAddress.Parse(Settings.Default.smarteye_ip_address), SMARTEYE_PORT_LATENT);
                LatentClient = new System.Net.Sockets.TcpClient();
                LatentClient.Connect(latentEndpoint);
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't open filtered data port (TCP) for SmartEye: " + e);
                LatentClient = null;
            }

            TrackerInit();
        }

        //Retrieve the World Model and calibration error vectors. Returns true on success
        private Boolean GetWorldCalibration()
        {
            //Retrieve WorldModel

            SendRpc(new SERPC("getWorldModel").GetNetstring());
            JToken result = ReceiveRpcResponse().GetValue("result");

            //Escaped String representing the world model
            WorldModelString = result.Value<String>("worldModel");

            List<SETarget> targets = new List<SETarget>();

            //Retrieve calibration targets
            int targetNumber = 0;
            while (true)
            {
                SendRpc(new SERPCGetTargetStats(targetNumber++).GetNetstring());
                SETarget target = ReceiveRpcResponse().GetValue("result").ToObject<SETarget>();

                if (!target.TargetValid())
                    break;

                targets.Add(target);
            }

            if (targets.Count == 0)
            {
                //No targets, most likely user has not done a gaze calibration in SmartEye
                System.Windows.Forms.MessageBox.Show("SmartEye is connected, but reported no calibration point data. Perform a gaze calibration from the SmartEye software.");
                return false;
            }

            //Store world model string and calibration data
            SmartEyeCalibrationResult seCalibrationResult = new SmartEyeCalibrationResult(WorldModelString, targets);
            SessionManager.GetInstance().SetCalibration(seCalibrationResult, this);

            return true;
        }

        public JObject ReceiveRpcResponse()
        {
            System.Net.Sockets.NetworkStream recvStream = RpcClient.GetStream();
            recvStream.Read(recvBuffer, 0, RpcClient.ReceiveBufferSize);          
            string response = Encoding.UTF8.GetString(recvBuffer);
            response = NetstringUtils.TrimSENetstring(response.TrimEnd('\0'));
            return JsonConvert.DeserializeObject<dynamic>(response);
        }

        public bool TrackerFound()
        {
            return (RealtimeClient != null) && (RpcClient != null);
        }

        public String GetTrackerName()
        {
            return TrackerName;
        }

        public String GetTrackerSerialNumber()
        {
            return TrackerSerialNumber;
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
            SendRpc(new SERPC("startTracking").GetNetstring());

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
            SendRpc(new SERPC("stopTracking").GetNetstring());

            Listen = false;
        }

        public void EnterCalibration()
        {          
            //Open calibration window (not working yet)
            //SendRpc(new SERPC("calibrateGaze").GetNetstring());

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
