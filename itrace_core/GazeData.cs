using System;
using System.Windows.Forms;
using System.Xml;

namespace iTrace_Core
{
    public abstract class GazeData
    {
        // Data sent to plugins
        public int? X { get; protected set; }
        public int? Y { get; protected set; }

        // Data recorded for the right eye
        public double RightX { get; protected set; }
        public double RightY { get; protected set; }
        public double RightPupil { get; protected set; }
        public int RightValidation { get; protected set; }

        // Data recorded for the left eye
        public double LeftX { get; protected set; }
        public double LeftY { get; protected set; }
        public double LeftPupil { get; protected set; }
        public int LeftValidation { get; protected set; }

        // Data recorded for user space coordinates
        public double UserRightX { get; protected set; }
        public double UserRightY { get; protected set; }
        public double UserRightZ { get; protected set; }
        public double UserLeftX { get; protected set; }
        public double UserLeftY { get; protected set; }
        public double UserLeftZ { get; protected set; }

        // Used to synchronize data sent between tracker and plugin
        public long EventTime { get; protected set; }

        // Collected from the hardware tracker
        public long TrackerTime { get; protected set; }

        // General time for post processing
        public long SystemTime { get; protected set; }

        public GazeData()
        {
            X = null;
            Y = null;

            RightX = 0;
            RightY = 0;
            RightPupil = 0;
            RightValidation = 0;

            LeftX = 0;
            LeftY = 0;
            LeftPupil = 0;
            LeftValidation = 0;

            UserLeftX = 0;
            UserLeftY = 0;
            UserLeftZ = 0;

            UserRightX = 0;
            UserRightY = 0;
            UserRightZ = 0;

            //Should be high resolution, but the offset is probably from the .NET epoch (DateTime.MinValue)
            //EventTime = DateTime.UtcNow.Ticks;
            EventTime = PreciseSystemTime.GetTime();

            //Should be decent enough resolution for post processing calculations
            SystemTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            TrackerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public String Output()
        {
            return "GAZE: (" + X + ", " + Y + ")";
        }

        public String Serialize()
        {
            return "gaze," + EventTime.ToString() + "," + X.ToString() + "," + Y.ToString() + "\n";
        }

        // Compute validity based on tracker data
        //   If either of the points are valid, take them
        public bool IsValid()
        {
            return Convert.ToBoolean(RightValidation) || Convert.ToBoolean(LeftValidation);
        }
    }

    public class TobiiGazeData : GazeData
    {
        public TobiiGazeData(Tobii.Research.GazeDataEventArgs tobiiRawGaze) : base()
        {
            bool isLeftEyeValid = tobiiRawGaze.LeftEye.GazePoint.Validity == Tobii.Research.Validity.Valid;
            bool isRightEyeValid = tobiiRawGaze.RightEye.GazePoint.Validity == Tobii.Research.Validity.Valid;

            if (isLeftEyeValid && isRightEyeValid)
            {
                double avgX = (tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.X + tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.X) / 2.0D;
                double avgY = (tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.Y + tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.Y) / 2.0D;

                X = Convert.ToInt32(avgX * Screen.PrimaryScreen.Bounds.Width);
                Y = Convert.ToInt32(avgY * Screen.PrimaryScreen.Bounds.Height);

            }
            else if (isLeftEyeValid)
            {
                X = Convert.ToInt32(tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.X * Screen.PrimaryScreen.Bounds.Width);
                Y = Convert.ToInt32(tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.Y * Screen.PrimaryScreen.Bounds.Height);
            }
            else if (isRightEyeValid)
            {
                X = Convert.ToInt32(tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.X * Screen.PrimaryScreen.Bounds.Width);
                Y = Convert.ToInt32(tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.Y * Screen.PrimaryScreen.Bounds.Height);
            }
            else
            {
                //Both eyes invalid.
                X = null;
                Y = null;
            }

            RightX = tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.X * Screen.PrimaryScreen.Bounds.Width;
            RightY = tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.Y * Screen.PrimaryScreen.Bounds.Height;
            RightPupil = tobiiRawGaze.RightEye.Pupil.PupilDiameter;
            RightValidation = Convert.ToInt32(isRightEyeValid);

            LeftX = tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.X * Screen.PrimaryScreen.Bounds.Width;
            LeftY = tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.Y * Screen.PrimaryScreen.Bounds.Height;
            LeftPupil = tobiiRawGaze.LeftEye.Pupil.PupilDiameter;
            LeftValidation = Convert.ToInt32(isLeftEyeValid);

            UserLeftX = tobiiRawGaze.LeftEye.GazeOrigin.PositionInUserCoordinates.X;
            UserLeftY = tobiiRawGaze.LeftEye.GazeOrigin.PositionInUserCoordinates.Y;
            UserLeftZ = tobiiRawGaze.LeftEye.GazeOrigin.PositionInUserCoordinates.Z;

            UserRightX = tobiiRawGaze.RightEye.GazeOrigin.PositionInUserCoordinates.X;
            UserRightY = tobiiRawGaze.RightEye.GazeOrigin.PositionInUserCoordinates.Y;
            UserRightZ = tobiiRawGaze.RightEye.GazeOrigin.PositionInUserCoordinates.Z;
            
            TrackerTime = tobiiRawGaze.DeviceTimeStamp;
        }
    }

    public class MouseTrackerGazeData : GazeData
    {
        public MouseTrackerGazeData(int mousePosX, int mousePosY) : base()
        {
            X = mousePosX;
            Y = mousePosY;

            // Same values as X and Y
            RightX = mousePosX;
            RightY = mousePosY;
            LeftX = mousePosX;
            LeftY = mousePosY;

            // Points are always valid (they just might not be within a plugin compatible window)
            RightValidation = 1;
            LeftValidation = 1;
        }
    }

    public class SmartEyeGazeData : GazeData
    {
        private const UInt16 SEFilteredClosestWorldIntersectionId = 0x0041;
        private const UInt16 SEFilteredLeftPupilDiameter = 0x0068;
        private const UInt16 SEFilteredRightPupilDiameter = 0x006A;
        private const UInt16 SEFilteredLeftClosestWorldIntersection = 0x00B6;
        private const UInt16 SEFilteredRightClosestWorldIntersection = 0x00B8;

        private const int SEType_u16_Size = 2;
        private const int SEType_u32_Size = 4;
        private const int SEType_f64_Size = 8;

        public SmartEyeGazeData(byte[] packet)
        {
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

                Console.WriteLine("\tSubpacketType: 0x{0:X} Length: {1}", SubpacketId, SubpacketLength);

                Int32 SubpacketOffset = Index;

                //Look for a left right or combined world intersection subpacket
                if (SubpacketId == SEFilteredClosestWorldIntersectionId ||
                    SubpacketId == SEFilteredLeftClosestWorldIntersection ||
                    SubpacketId == SEFilteredRightClosestWorldIntersection)
                {

                    //Check if an intersection exists, the first U16 will be 1 if this is the case.
                    if (ParseSEType_u16(packet, SubpacketOffset) == 1)
                    {
                        int x;
                        int y;

                        GetScreenCoordsFromWorldIntersection(packet, SubpacketOffset, out x, out y);

                        //Skip over fields, need a better way of conveying where things are
                        SubpacketOffset += SEType_u16_Size + 6 * SEType_f64_Size;

                        //Read name of intersected object
                        UInt16 intersectNameLength = ParseSEType_u16(packet, SubpacketOffset);

                        SubpacketOffset += SEType_u16_Size;

                        String intersectName = System.Text.Encoding.ASCII.GetString(packet, SubpacketOffset, intersectNameLength);

                        switch (SubpacketId)
                        {
                            case SEFilteredClosestWorldIntersectionId:
                                this.X = x;
                                this.Y = y;
                                Console.WriteLine("Combined Intersection \"{0}\" at coords {1}, {2}", intersectName, x, y);
                                break;

                            case SEFilteredLeftClosestWorldIntersection:
                                this.LeftX = x;
                                this.LeftY = y;
                                this.LeftValidation = 1;
                                Console.WriteLine("Left Intersection \"{0}\" at coords {1}, {2}", intersectName, x, y);
                                break;

                            case SEFilteredRightClosestWorldIntersection:
                                this.RightX = x;
                                this.RightY = y;
                                this.RightValidation = 1;
                                Console.WriteLine("Right Intersection \"{0}\" at coords {1}, {2}", intersectName, x, y);
                                break;
                        }
                    }
                }


                if (SubpacketId == SEFilteredLeftPupilDiameter || SubpacketId == SEFilteredRightPupilDiameter)
                {
                    double diam = ParseSEType_f64(packet, SubpacketOffset);

                    switch (SubpacketId)
                    {
                        case SEFilteredLeftPupilDiameter:
                            this.LeftPupil = diam;
                            Console.WriteLine("Left Pupil: {0}", diam);
                            break;

                        case SEFilteredRightPupilDiameter:
                            this.RightPupil = diam;
                            Console.WriteLine("Right Pupil: {0}", diam);
                            break;
                    }
                }

                //Advance to the next Subpacket
                Index += SubpacketLength;
            }
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
    }

    public class GazepointGazeData : GazeData
    {
        public GazepointGazeData(String gazePointRawGaze) : base()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(gazePointRawGaze);

            XmlNode recNode = xmlDoc.FirstChild;

            if (recNode.Attributes["BPOGX"] == null)
            {
                X = null;
                Y = null;
                return;
            }

            X = Convert.ToInt32(Double.Parse(recNode.Attributes["BPOGX"].Value) * Screen.PrimaryScreen.Bounds.Width);
            Y = Convert.ToInt32(Double.Parse(recNode.Attributes["BPOGY"].Value) * Screen.PrimaryScreen.Bounds.Height);

            RightX = Double.Parse(recNode.Attributes["RPOGX"].Value) * Screen.PrimaryScreen.Bounds.Width;
            RightY = Double.Parse(recNode.Attributes["RPOGY"].Value) * Screen.PrimaryScreen.Bounds.Height;
            RightPupil = Double.Parse(recNode.Attributes["RPD"].Value);
            RightValidation = Int32.Parse(recNode.Attributes["RPOGV"].Value);

            LeftX = Double.Parse(recNode.Attributes["LPOGX"].Value) * Screen.PrimaryScreen.Bounds.Width;
            LeftY = Double.Parse(recNode.Attributes["LPOGY"].Value) * Screen.PrimaryScreen.Bounds.Height;
            LeftPupil = Double.Parse(recNode.Attributes["LPD"].Value);
            LeftValidation = Int32.Parse(recNode.Attributes["LPOGV"].Value);

            UserLeftX = Double.Parse(recNode.Attributes["LEYEX"].Value);
            UserLeftY = Double.Parse(recNode.Attributes["LEYEY"].Value);
            UserLeftZ = Double.Parse(recNode.Attributes["LEYEZ"].Value);

            UserRightX = Double.Parse(recNode.Attributes["REYEX"].Value);
            UserRightY = Double.Parse(recNode.Attributes["REYEY"].Value); ;
            UserRightZ = Double.Parse(recNode.Attributes["REYEZ"].Value); ;

            TrackerTime = Convert.ToInt64(recNode.Attributes["TIME_TICK"].Value);
        }
    }
}
