using System;
using System.Net;
using System.Text;

namespace iTrace_Core
{
    class SmartEyeTracker : ITracker
    {
        private readonly String SMARTEYE_ADDRESS = "192.168.100.45"; //TODO option to select?
        private readonly int SMARTEYE_PORT = 5800; //TODO set to default from SE software
        private System.Net.Sockets.UdpClient Client;
        private IPEndPoint ep;
        private String TrackerName;
        private String TrackerSerialNumber;

        private bool Listen;

        public SmartEyeTracker()
        {
            ep = new IPEndPoint(IPAddress.Parse(SMARTEYE_ADDRESS), SMARTEYE_PORT);

            TrackerName = "SmartEye Tracker";
            TrackerSerialNumber = "1234";

            try
            {
                Client = new System.Net.Sockets.UdpClient(ep);

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
            if (Client != null)
            {
                Console.WriteLine("Client connected");
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
        private UInt16 ConvertEndian32(UInt16 value)
        {
            if (!BitConverter.IsLittleEndian)
                return value;

            return (UInt16)( (value >> 24)
                | (value << 24)
                | ((value >> 8) & 0x0000FF00)
                | ((value << 8) & 0x00FF0000) );
        }

        private UInt16 ConvertEndian16(UInt16 value)
        {
            if (!BitConverter.IsLittleEndian)
                return value;

            return (UInt16)((value >> 8) | (value << 8));
        }

        private void ListenForData()
        {
            Listen = true;

            while (Listen)
            {
                //Receive UDP packet
                //TODO listen for termination

                byte[] packet = Client.Receive(ref ep);

                //Print packet header
                UInt16 PacketType = ConvertEndian16(BitConverter.ToUInt16(packet, 4));
                UInt16 PacketLength = ConvertEndian16(BitConverter.ToUInt16(packet, 6));
                Console.WriteLine("Packet Type: {0} Length: {1}", PacketType, PacketLength);

                //Print subpackets and their IDs
                //Start of first subpacket at 8 bytes
                Int32 Index = 8; 

                while (Index < PacketLength)
                {
                    //Pg 9 in the SmartEye Programmers Guide gives the following offsets to the Subpacket Id and Length
                    UInt16 SubpacketId = ConvertEndian16(BitConverter.ToUInt16(packet, Index));
                    UInt16 SubpacketLength = ConvertEndian16(BitConverter.ToUInt16(packet, Index + 2));

                    //Advance to the next Subpacket
                    Index += 4 + SubpacketLength;

                    Console.WriteLine("\tSubpacketType: 0x{0:X} Length: {1}", SubpacketId, SubpacketLength);
                }
            }
        }
    }
}
