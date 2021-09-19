using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using iTrace_Core.Properties;
using Newtonsoft.Json.Linq;

namespace iTrace_Core
{
    class SmartEyeTracker : ITracker
    {
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
        private String WorldModelString;

        private bool Listen;

        public SmartEyeTracker()
        {
            TrackerName = "SmartEye Tracker";
            TrackerSerialNumber = "1234";

            //TODO catch parse exception?
            IPAddress rpcAddress = IPAddress.Parse(Settings.Default.smarteye_ip_address);
            rpcEndpoint = new IPEndPoint(rpcAddress, SMARTEYE_PORT_RPC);
            
            try
            {
                //Try to connect to the RPC server on SmartEye host machine
                RpcClient = new System.Net.Sockets.TcpClient();
                RpcClient.Connect(rpcEndpoint);

                byte[] recvBuffer = new byte[RpcClient.ReceiveBufferSize];
                System.Net.Sockets.NetworkStream recvStream = RpcClient.GetStream();

                SendRpc(new SERPC("getState").GetNetstring());
                recvStream.Read(recvBuffer, 0, RpcClient.ReceiveBufferSize);

                //Netstrings test
                String response = Encoding.UTF8.GetString(recvBuffer);
                response = NetstringUtils.TrimSENetstring(response.TrimEnd('\0'));

                //Does not deserialize the response correctly yet
                SERPCGetStateResponse state = JsonConvert.DeserializeObject<SERPCGetStateResponse>(response);
                
                Console.WriteLine("JSON response: {0}\n", response);

                //Retrieve WorldModel and configuration

                SendRpc(new SERPC("getWorldModel").GetNetstring());
                recvStream.Read(recvBuffer, 0, RpcClient.ReceiveBufferSize);

                String response2 = Encoding.UTF8.GetString(recvBuffer);
                response2 = NetstringUtils.TrimSENetstring(response2.TrimEnd('\0'));

                JObject CmdResponse = JsonConvert.DeserializeObject<dynamic>(response2);
                JToken result = CmdResponse.GetValue("result");

                //Escaped String representing the world model
                WorldModelString = result.Value<String>("worldModel");

                SessionManager.GetInstance().SetCalibration(new SmartEyeCalibrationResult(WorldModelString), this);
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
