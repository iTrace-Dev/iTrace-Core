/********************************************************************************************************************************************************
* @file SmartEyeTracker.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using iTrace_Core.Properties;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Forms;

namespace iTrace_Core
{
    class SmartEyeTracker : ITracker
    {
        private readonly int SMARTEYE_PORT_LATENT = 5799;
        private readonly int SMARTEYE_PORT_RPC = 8100; //This is the default from SE software

        private System.Net.Sockets.UdpClient RealtimeClient;
        private System.Net.Sockets.TcpClient LatentClient;
        private System.Net.Sockets.TcpClient RpcClient;
        private IPEndPoint realtimeEndpoint;    //For real time data
        private IPEndPoint latentEndpoint;      //For processed/filtered data
        private IPEndPoint rpcEndpoint;      //For sending json RPC commands to SmartEye
        private String TrackerName;
        private String TrackerSerialNumber;

        public SmartEyeCalibrationResult seCalibrationResult { get; private set; }

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
                Console.WriteLine("Attempting to connect to Smarteye...");
                RpcClient = new System.Net.Sockets.TcpClient();
                IAsyncResult conn = RpcClient.BeginConnect(rpcEndpoint.Address, rpcEndpoint.Port, null, null);

                bool rpcConnectSuccess = conn.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

                if (!rpcConnectSuccess)
                    throw new Exception("Rpc connection timed out");

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

            IPAddress selfAddress = null;
            try
            {
                IPHostEntry hostname = Dns.GetHostEntry(Dns.GetHostName());
                //Search our IP addresses for one on the same network as the smarteye server
                
                //TODO: test if this gives the same selfAddress
                IPEndPoint iep = (IPEndPoint)RpcClient.Client.LocalEndPoint;
                Console.WriteLine($"SmartEyeTracker Own (iTrace machine) Address is: {iep.Address}");
                selfAddress = iep.Address;

                if (selfAddress == null)
                {
                    Console.WriteLine("Not connected to the same network as the specified SmartEye server!");
                    RealtimeClient = null;
                    return;
                }

                //Try pinging the SmartEye host computer
                using (System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping())
                {
                    System.Net.NetworkInformation.PingReply reply = p.Send(rpcAddress);
                    if (reply.Status != System.Net.NetworkInformation.IPStatus.Success)
                        throw new Exception("Provided SmartEye IP address is not responding.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem connecting to SmartEye: " + e);
                RealtimeClient = null;
                return;
            }

            try
            {
                realtimeEndpoint = new IPEndPoint(selfAddress, Settings.Default.smarteye_ip_port);
                RealtimeClient = new System.Net.Sockets.UdpClient(realtimeEndpoint);

                //TODO: Configure SmartEye output settings here

            } catch (Exception e)
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

                //TODO: Configure SmartEye output settings here

            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't open filtered data port (TCP) for SmartEye: " + e);
                LatentClient = null;
            }

            TrackerInit();
        }

        //Disconnect any connections, performed before refreshing the tracker list
        public void CleanupConnections()
        {
            if (LatentClient.Connected)
                LatentClient.Close();

            if (RpcClient.Connected)
                RpcClient.Close();

            RealtimeClient.Close();
        }

        //Retrieve the World Model and calibration error vectors. Returns true on success
        private Boolean GetWorldCalibration()
        {
            //Retrieve WorldModel

            SendRpc(new SERPC("getWorldModel").GetNetstring());
            JToken result = ReceiveRpcResponse().GetValue("result");

            //Escaped String representing the world model
            String WorldModelString = result.Value<String>("worldModel");

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
            SEWorldModel worldModel = new SEWorldModel(WorldModelString);
            seCalibrationResult = new SmartEyeCalibrationResult(worldModel, targets);
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

                if (gaze.IsValid()) //Sending gaze data that isn't valid breaks the plugins?
                {
                    //TODO: Adjust coordinates based on Screens
                    //Converts Screen space coords to one space as used by OS
                    //Needed in order for plugins to recognize multiple screens

                    bool hasIntersection = !String.IsNullOrWhiteSpace(gaze.intersectionName);
                    bool hasXY = (gaze.X != null) && (gaze.Y != null);

                    if (hasIntersection && hasXY)
                    {
                        Screen targetScreen = seCalibrationResult.screenMapping.GetSEToScreenMapping(gaze.intersectionName);

                        if (targetScreen != null)
                            gaze.Offset(targetScreen.Bounds.X, targetScreen.Bounds.Y);

                        GazeHandler.Instance.EnqueueGaze(gaze);
                    }
                }
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
