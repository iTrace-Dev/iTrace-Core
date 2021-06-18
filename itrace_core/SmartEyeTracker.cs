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

        private const int SEFilteredClosestWorldIntersectionId = 0x41;

        private const int SEType_u16_Size = 2;
        private const int SEType_u32_Size = 4;
        private const int SEType_f64_Size = 8;

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

        //Parse a UInt16 from an SmartEye packet, accounting for endianness
        private UInt16 ParseSEType_u16(byte[] packet, Int32 offset)
        {
            byte[] bytes = new byte[SEType_u16_Size];
            Array.ConstrainedCopy(packet, offset, bytes, 0, SEType_u16_Size);

            //Reverse bytes if system endianness is not Big
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToUInt16(bytes, 0);
        }

        private UInt32 ParseSEType_u32(byte[] packet, Int32 offset)
        {
            byte[] bytes = new byte[SEType_u32_Size];
            Array.ConstrainedCopy(packet, offset, bytes, 0, SEType_u32_Size);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToUInt32(bytes, 0);
        }

        private double ParseSEType_f64(byte[] packet, Int32 offset)
        {
            byte[] bytes = new byte[SEType_f64_Size];
            Array.ConstrainedCopy(packet, offset, bytes, 0, SEType_f64_Size);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToDouble(bytes, 0);
        }

        //Get X and Y coordinates from an SEType_WorldIntersection
        private void GetScreenCoordsFromWorldIntersection(byte[] packet, Int32 offset, out int x, out int y)
        {
            //Skip over fields SEType_u16 intersections, and SEType_Point3D worldPoint (3 floats of 8 bytes each)
            offset += SEType_u16_Size + 3 * SEType_f64_Size;

            x = (int)ParseSEType_f64(packet, offset);
            y = (int)ParseSEType_f64(packet, offset + SEType_f64_Size);
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
                UInt16 PacketType = ParseSEType_u16(packet, 4);
                UInt16 PacketLength = ParseSEType_u16(packet, 6);
                Console.WriteLine("Packet Type: {0} Length: {1}", PacketType, PacketLength);

                //Print subpackets and their IDs
                //Start of first subpacket at 8 bytes
                Int32 Index = 8; 

                while (Index < PacketLength)
                {
                    //Pg 9 in the SmartEye Programmers Guide gives the following offsets to the Subpacket Id and Length
                    UInt16 SubpacketId = ParseSEType_u16(packet, Index);
                    UInt16 SubpacketLength = ParseSEType_u16(packet, Index + SEType_u16_Size);

                    //Advance beyond the 4 byte packet header
                    Index += 2 * SEType_u16_Size;

                   //Console.WriteLine("\tSubpacketType: 0x{0:X} Length: {1}", SubpacketId, SubpacketLength);

                    //Look for the field SEFilteredClosestWorldIntersection, which has ID 0x41
                    if (SubpacketId == SEFilteredClosestWorldIntersectionId)
                    {
                        Int32 SubpacketOffset = Index;

                        //Check if an intersection exists, the first U16 will be 1 if this is the case.
                        if (ParseSEType_u16(packet, SubpacketOffset) == 1)
                        {
                            int x;
                            int y;

                            GetScreenCoordsFromWorldIntersection(packet, SubpacketOffset, out x, out y);

                            //Read coordinates from field SEType_Point3D objectPoint
                            double objectIntersectionX = ParseSEType_f64(packet, SubpacketOffset);
                            double objectIntersectionY = ParseSEType_f64(packet, SubpacketOffset + SEType_f64_Size);
                            double objectIntersectionZ = ParseSEType_f64(packet, SubpacketOffset + 2 * SEType_f64_Size);

                            //Skip over fields, need a better way of conveying where things are
                            SubpacketOffset += SEType_u16_Size + 6 * SEType_f64_Size;

                            //Read name of intersected object
                            UInt16 intersectNameLength = ParseSEType_u16(packet, SubpacketOffset);

                            SubpacketOffset += SEType_u16_Size;

                            String intersectName = Encoding.ASCII.GetString(packet, SubpacketOffset, intersectNameLength);

                            Console.WriteLine("Intersection \"{0}\" at coords {1}, {2}", intersectName, x, y);

                            //Move this SmartEyeGazeData and gradually fill it as subpackets are encountered
                            GazeHandler.Instance.EnqueueGaze(new SmartEyeGazeData(x, y));
                        }
                    }

                    //Advance to the next Subpacket
                    Index += SubpacketLength;
                }
            }
        }
    }
}
