/********************************************************************************************************************************************************
* @file GazeData.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

using iTrace_Core.Properties;
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
            Screen screen = Screen.AllScreens[Settings.Default.calibration_monitor];

            RightX = tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.X * screen.Bounds.Width + screen.Bounds.Left;
            RightY = tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.Y * screen.Bounds.Height + screen.Bounds.Top;
            RightPupil = tobiiRawGaze.RightEye.Pupil.PupilDiameter;
            RightValidation = Convert.ToInt32(isRightEyeValid);

            LeftX = tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.X * screen.Bounds.Width + screen.Bounds.Left;
            LeftY = tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.Y * screen.Bounds.Height + screen.Bounds.Top;
            LeftPupil = tobiiRawGaze.LeftEye.Pupil.PupilDiameter;
            LeftValidation = Convert.ToInt32(isLeftEyeValid);

            if (isLeftEyeValid && isRightEyeValid)
            {
                double avgX = (RightX + LeftX) / 2.0D;
                double avgY = (RightY + LeftY) / 2.0D;

                X = Convert.ToInt32(avgX);
                Y = Convert.ToInt32(avgY);
            }
            else if (isLeftEyeValid)
            {
                X = Convert.ToInt32(LeftX);
                Y = Convert.ToInt32(LeftY);
            }
            else if (isRightEyeValid)
            {
                X = Convert.ToInt32(RightX);
                Y = Convert.ToInt32(RightY);
            }
            else
            {
                //Both eyes invalid.
                X = null;
                Y = null;
            }
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
        //Debug messages
        private const bool PrintPacketInfo = false;
        private const bool PrintIntersectionInfo = true;
        private const bool PrintPupilInfo = false;

        //Lot of constants
        //All subpacket IDs are uint16s
        //These are in the same order as in the ProgrammersGuide, not in order of ID
        private const UInt16 SEFrameNumber = 0x0001;
        private const UInt16 SEEstimatedDelay = 0x0002;
        private const UInt16 SETimeStamp = 0x0003;
        private const UInt16 SEUserTimeStamp = 0x0004;
        private const UInt16 SEFrameRate = 0x0005;
        private const UInt16 SECameraPositions = 0x0006;
        private const UInt16 SECameraRotations = 0x0007;
        private const UInt16 SEUserDefinedData = 0x0008;
        private const UInt16 SERealTimeClock = 0x0009;
        private const UInt16 SEHeadPosition = 0x0010; //Yes, 0x10
        private const UInt16 SEHeadPositionQ = 0x0011;
        private const UInt16 SEHeadRotationRodrigues = 0x0012;
        private const UInt16 SEHeadRotationQuaternion = 0x001D;
        private const UInt16 SEHeadLeftEarDirection = 0x0015;
        private const UInt16 SEHeadUpDirection = 0x0014;
        private const UInt16 SEHeadNoseDirection = 0x0013;
        private const UInt16 SEHeadHeading = 0x0016;
        private const UInt16 SEHeadPitch = 0x0017;
        private const UInt16 SEHeadRoll = 0x0018;
        private const UInt16 SEHeadRotationQ = 0x0019;
        private const UInt16 SEGazeOrigin = 0x001A;
        private const UInt16 SELeftGazeOrigin = 0x001B;
        private const UInt16 SERightGazeOrigin = 0x001C;
        private const UInt16 SEEyePosition = 0x0020;
        private const UInt16 SEGazeDirection = 0x0021;
        private const UInt16 SEGazeDirectionQ = 0x0022;
        private const UInt16 SELeftEyePosition = 0x0023;
        private const UInt16 SELeftGazeDirection = 0x0024;
        private const UInt16 SELeftGazeDirectionQ = 0x0025;
        private const UInt16 SERightEyePosition = 0x0026;
        private const UInt16 SERightGazeDirection = 0x0027;
        private const UInt16 SERightGazeDirectionQ = 0x0028;
        private const UInt16 SEGazeHeading = 0x0029;
        private const UInt16 SEGazePitch = 0x002A;
        private const UInt16 SELeftGazeHeading = 0x002B;
        private const UInt16 SELeftGazePitch = 0x002C;
        private const UInt16 SERightGazeHeading = 0x002D;
        private const UInt16 SERightGazePitch = 0x002E;
        private const UInt16 SEFilteredGazeDirection = 0x0030;
        private const UInt16 SEFilteredLeftGazeDirection = 0x0032;
        private const UInt16 SEFilteredRightGazeDirection = 0x0034;
        private const UInt16 SEFilteredGazeHeading = 0x0036;
        private const UInt16 SEFilteredGazePitch = 0x0037;
        private const UInt16 SEFilteredLeftGazeHeading = 0x0038;
        private const UInt16 SEFilteredLeftGazePitch = 0x0039;
        private const UInt16 SEFilteredRightGazeHeading = 0x003A;
        private const UInt16 SEFilteredRightGazePitch = 0x003B;
        private const UInt16 SESaccade = 0x003D;
        private const UInt16 SEFixation = 0x003E;
        private const UInt16 SEBlink = 0x003F;
        private const UInt16 SEClosestWorldIntersection = 0x0040;
        private const UInt16 SEFilteredClosestWorldIntersectionId = 0x0041;
        private const UInt16 SEAllWorldIntersections = 0x0042;
        private const UInt16 SEFilteredAllWorldIntersections = 0x0043;
        private const UInt16 SEZoneId = 0x0044;
        private const UInt16 SEEstimatedClosestWorldIntersection = 0x0045;
        private const UInt16 SEEstimatedAllWorldIntersections = 0x0046;
        private const UInt16 SEHeadClosestWorldIntersection = 0x0049;
        private const UInt16 SEHeadAllWorldIntersections = 0x004A;
        private const UInt16 SEEyelidOpening = 0x0050;
        private const UInt16 SEEyelidOpeningQ = 0x0051;
        private const UInt16 SELeftEyelidOpening = 0x0052;
        private const UInt16 SELeftEyelidOpeningQ = 0x0053;
        private const UInt16 SERightEyelidOpening = 0x0054;
        private const UInt16 SERightEyelidOpeningQ = 0x0055;
        private const UInt16 SEKeyboardState = 0x0056;
        private const UInt16 SELeftLowerEyelidExtremePoint = 0x0058;
        private const UInt16 SELeftUpperEyelidExtremePoint = 0x0059;
        private const UInt16 SERightLowerEyelidExtremePoint = 0x005A;
        private const UInt16 SERightUpperEyelidExtremePoint = 0x005B;
        private const UInt16 SEPupilDiameter = 0x0060;
        private const UInt16 SEPupilDiameterQ = 0x0061;
        private const UInt16 SELeftPupilDiameter = 0x0062;
        private const UInt16 SELeftPupilDiamterQ = 0x0063;
        private const UInt16 SERightPupilDiameter = 0x0064;
        private const UInt16 SERightPupilDiameterQ = 0x0065;
        private const UInt16 SEFilteredPupilDiameter = 0x0066;
        private const UInt16 SEFilteredPupilDiameterQ = 0x0067;
        private const UInt16 SEFilteredLeftPupilDiameter = 0x0068;
        private const UInt16 SEFilteredLeftPupilDiamterQ = 0x0069;
        private const UInt16 SEFilteredRightPupilDiameter = 0x006A;
        private const UInt16 SEFilteredERightPupilDiameterQ = 0x006B;
        private const UInt16 SEGPSPosition = 0x0070;
        private const UInt16 SEGPSGroundSpeed = 0x0071;
        private const UInt16 SEGPSCourse = 0x0072;
        private const UInt16 SEGPSTime = 0x0073;
        private const UInt16 SEEstimatedGazeOrigin = 0x007A;
        private const UInt16 SEEstimatedLeftGazeOrigin = 0x007B;
        private const UInt16 SEEstimatedRightGazeOrigin = 0x007C;
        private const UInt16 SEEstimatedEyePosition = 0x0080;
        private const UInt16 SEEstimatedGazeDirection = 0x0081;
        private const UInt16 SEEstimatedGazeDirectionQ = 0x0082;
        private const UInt16 SEEstimatedGazeHeading = 0x0083;
        private const UInt16 SEEstimatedGazePitch = 0x0084;
        private const UInt16 SEEstimatedLeftEyePosition = 0x0085;
        private const UInt16 SEEstimatedLeftGazeDirection = 0x0086;
        private const UInt16 SEEstimatedLeftGazeDirectionQ = 0x0087;
        private const UInt16 SEEstimatedLeftGazeHeading = 0x0088;
        private const UInt16 SEEstimatedLeftGazePitch = 0x0089;
        private const UInt16 SEEstimatedRightEyePosition = 0x008A;
        private const UInt16 SEEstimatedRightGazeDirection = 0x008B;
        private const UInt16 SEEstimatedRightGazeDirectionQ = 0x008C;
        private const UInt16 SEEstimatedRightGazeHeading = 0x008D;
        private const UInt16 SEEstimatedRightGazePitch = 0x008E;
        private const UInt16 SEFilteredEstimatedGazeDirection = 0x0091;
        private const UInt16 SEFilteredEstimatedGazeHeading = 0x0093;
        private const UInt16 SEFilteredEstimatedGazePitch = 0x0094;
        private const UInt16 SEFilteredEstimatedLeftGazeDirection = 0x0096;
        private const UInt16 SEFilteredEstimatedLeftGazeHeading = 0x0098;
        private const UInt16 SEFilteredEstimatedLeftGazePitch = 0x0099;
        private const UInt16 SEFilteredEstimatedRightGazeDirection = 0x009B;
        private const UInt16 SEFilteredEstimatedRightGazeHeading = 0x009D;
        private const UInt16 SEFilteredEstimatedRightGazePitch = 0x009E;
        private const UInt16 SEASCIIKeyboardState = 0x00A4;
        private const UInt16 SECalibrationGazeIntersection = 0x00B0;
        private const UInt16 SETaggedGazeIntersection = 0x00B1;
        private const UInt16 SELeftClosestWorldIntersection = 0x00B2;
        private const UInt16 SELeftAllWorldIntersections = 0x00B3;
        private const UInt16 SERightClosestWorldIntersection = 0x00B4;
        private const UInt16 SERightAllWorldIntersections = 0x00B5;
        private const UInt16 SEFilteredLeftClosestWorldIntersection = 0x00B6;
        private const UInt16 SEFilteredLeftAllWorldIntersections = 0x00B7;
        private const UInt16 SEFilteredRightClosestWorldIntersection = 0x00B8;
        private const UInt16 SEFilteredRightAllWorldIntersections = 0x00B9;
        private const UInt16 SEEstimatedLeftClosestWorldIntersection = 0x00BA;
        private const UInt16 SEEstimatedLeftAllWorldIntersections = 0x00BB;
        private const UInt16 SEEstimatedRightClosestWorldIntersection = 0x00BC;
        private const UInt16 SEEstimatedRightAllWorldIntersections = 0x00BD;
        private const UInt16 SEOptimalReflexReductionMode = 0x00C3;
        private const UInt16 SELeftBlinkClosingMidTime = 0x00E0;
        private const UInt16 SELeftBlinkOpeningMidTime = 0x00E1;
        private const UInt16 SELeftBlinkClosingAmplitude = 0x00E2;
        private const UInt16 SELeftBlinkOpeningAmplitude = 0x00E3;
        private const UInt16 SELeftBlinkClosingSpeed = 0x00E4;
        private const UInt16 SELeftBlinkOpeningSpeed = 0x00E5;
        private const UInt16 SERightBlinkClosingMidTime = 0x00E6;
        private const UInt16 SERightBlinkOpeningMidTime = 0x00E7;
        private const UInt16 SERightBlinkClosingAmplitude = 0x00E8;
        private const UInt16 SERightBlinkOpeningAmplitude = 0x00E9;
        private const UInt16 SERightBlinkClosingSpeed = 0x00EA;
        private const UInt16 SERightBlinkOpeningSpeed = 0x00EB;
        private const UInt16 SELeftEyeOuterCorner3D = 0x0300;
        private const UInt16 SELeftEyeInnerCorner3D = 0x0301;
        private const UInt16 SERightEyeInnerCorner3D = 0x0302;
        private const UInt16 SERightEyeOuterCorner3D = 0x0303;
        private const UInt16 SELeftNostril3D = 0x0304;
        private const UInt16 SERightNostril3D = 0x0305;
        private const UInt16 SELeftMouthCorner3D = 0x0306;
        private const UInt16 SERightMouthCorner3D = 0x0307;
        private const UInt16 SELeftEar3D = 0x0308;
        private const UInt16 SERightEar3D = 0x0309;
        private const UInt16 SENoseTip3D = 0x0360;
        private const UInt16 SELeftEyeOuterCorner2D = 0x0310;
        private const UInt16 SELeftEyeInnerCorner2D = 0x0311;
        private const UInt16 SERightEyeInnerCorner2D = 0x0312;
        private const UInt16 SERightEyeOuterCorner2D = 0x0313;
        private const UInt16 SELeftNostril2D = 0x0314;
        private const UInt16 SERightNostril2D = 0x0315;
        private const UInt16 SELeftMouthCorner2D = 0x0316;
        private const UInt16 SERightMouthCorner2D = 0x0317;
        private const UInt16 SELeftEar2D = 0x0318;
        private const UInt16 SERightEar2D = 0x0319;
        private const UInt16 SEIrisMatch = 0x0350;
        private const UInt16 SEPupilMatch = 0x0351;
        private const UInt16 SERobustIrisMatch = 0x0352;
        private const UInt16 SENoseTip2D = 0x0370;
        private const UInt16 SELowerEyelidPoints = 0x0380;
        private const UInt16 SEUpperEyelidPoints = 0x0381;
        private const UInt16 SELeftEyelidState = 0x0390;
        private const UInt16 SERightEyelidState = 0x0391;
        private const UInt16 SEUserMarker = 0x03A0;
        private const UInt16 SECameraClocks = 0x03A1;

        private const int SEType_u16_Size = 2;
        private const int SEType_u32_Size = 4;
        private const int SEType_u64_Size = 8;
        private const int SEType_f64_Size = 8;

        // TODO: these should be per-session
        private static UInt32 lastFixation = 0;
        private static UInt32 lastBlink = 0;

        // Intersection name used primarily for multiple screens
        public string intersectionName = "";

        public SmartEyeGazeData(byte[] packet)
        {
            //Print packet header
            UInt16 PacketType = ParseSEType_u16(packet, 4);
            UInt16 PacketLength = ParseSEType_u16(packet, 6);

            if (PrintPacketInfo)
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

                if (PrintPacketInfo)
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
                                this.intersectionName = intersectName;
                                if (PrintIntersectionInfo)
                                    Console.WriteLine("Combined Intersection \"{0}\" at coords {1}, {2}", intersectName, x, y);
                                break;

                            case SEFilteredLeftClosestWorldIntersection:
                                this.LeftX = x;
                                this.LeftY = y;
                                this.LeftValidation = 1;
                                if (PrintIntersectionInfo)
                                    Console.WriteLine("Left Intersection \"{0}\" at coords {1}, {2}", intersectName, x, y);
                                break;

                            case SEFilteredRightClosestWorldIntersection:
                                this.RightX = x;
                                this.RightY = y;
                                this.RightValidation = 1;
                                if (PrintIntersectionInfo)
                                    Console.WriteLine("Right Intersection \"{0}\" at coords {1}, {2}", intersectName, x, y);
                                break;
                        }
                    }
                }
                else if (SubpacketId == SETimeStamp)
                {
                    this.TrackerTime = (long)ParseSEType_u64(packet, SubpacketOffset);
                }
                else if (SubpacketId == SELeftGazeOrigin)
                {
                    this.UserLeftX = ParseSEType_f64(packet, SubpacketOffset);
                    this.UserLeftY = ParseSEType_f64(packet, SubpacketOffset + SEType_f64_Size);
                    this.UserLeftZ = ParseSEType_f64(packet, SubpacketOffset + 2 * SEType_f64_Size);
                }
                else if (SubpacketId == SERightGazeOrigin)
                {
                    this.UserRightX = ParseSEType_f64(packet, SubpacketOffset);
                    this.UserRightY = ParseSEType_f64(packet, SubpacketOffset + SEType_f64_Size);
                    this.UserRightZ = ParseSEType_f64(packet, SubpacketOffset + 2 * SEType_f64_Size);
                }
                else if (SubpacketId == SEFixation)
                {
                    UInt32 fixationNumber = ParseSEType_u32(packet, SubpacketOffset);

                    if (fixationNumber > lastFixation)
                    {
                        if (true) //TODO: Add toggle
                            Console.WriteLine("Fixation: {0}", fixationNumber);

                        lastFixation = fixationNumber;
                    }
                }
                else if (SubpacketId == SEBlink)
                {
                    UInt32 blinkNumber = ParseSEType_u32(packet, SubpacketOffset);

                    if (blinkNumber > lastBlink)
                    {
                        if (true) //TODO: Add toggle
                            Console.WriteLine("Blink: {0}", blinkNumber);

                        lastBlink = blinkNumber;
                    }
                }

                if (SubpacketId == SEFilteredLeftPupilDiameter || SubpacketId == SEFilteredRightPupilDiameter)
                {
                    double diam = ParseSEType_f64(packet, SubpacketOffset);

                    switch (SubpacketId)
                    {
                        case SEFilteredLeftPupilDiameter:
                            this.LeftPupil = diam;
                            if (PrintPupilInfo)
                                Console.WriteLine("Left Pupil: {0}", diam);
                            break;

                        case SEFilteredRightPupilDiameter:
                            this.RightPupil = diam;
                            if (PrintPupilInfo)
                                Console.WriteLine("Right Pupil: {0}", diam);
                            break;
                    }
                }

                //Advance to the next Subpacket
                Index += SubpacketLength;
            }
        }

        //Offset by screen position, for multi-screen use
        public void Offset(int x, int y)
        {
            this.X += x;
            this.Y += y;
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

        private UInt64 ParseSEType_u64(byte[] packet, Int32 offset)
        {
            byte[] bytes = new byte[SEType_u64_Size];
            Array.ConstrainedCopy(packet, offset, bytes, 0, SEType_u64_Size);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToUInt64(bytes, 0);
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
            Screen screen = Screen.AllScreens[Settings.Default.calibration_monitor];

            X = Convert.ToInt32(Double.Parse(recNode.Attributes["BPOGX"].Value) * screen.Bounds.Width + screen.Bounds.Left);
            Y = Convert.ToInt32(Double.Parse(recNode.Attributes["BPOGY"].Value) * screen.Bounds.Height + screen.Bounds.Top);

            RightX = Double.Parse(recNode.Attributes["RPOGX"].Value) * Screen.PrimaryScreen.Bounds.Width;
            RightY = Double.Parse(recNode.Attributes["RPOGY"].Value) * Screen.PrimaryScreen.Bounds.Height;
            RightValidation = Int32.Parse(recNode.Attributes["RPOGV"].Value);

            LeftX = Double.Parse(recNode.Attributes["LPOGX"].Value) * Screen.PrimaryScreen.Bounds.Width;
            LeftY = Double.Parse(recNode.Attributes["LPOGY"].Value) * Screen.PrimaryScreen.Bounds.Height;

            LeftValidation = Int32.Parse(recNode.Attributes["LPOGV"].Value);


            bool rightPupilValid = Int32.Parse(recNode.Attributes["RPUPILV"].Value) == 1;
            if (rightPupilValid)
            {
                RightPupil = Double.Parse(recNode.Attributes["RPUPILD"].Value) * 1000.0;
            }
            else
            {
                RightPupil = -1;
            }

            bool leftPupilValid = Int32.Parse(recNode.Attributes["LPUPILV"].Value) == 1;
            if (leftPupilValid)
            {
                LeftPupil = Double.Parse(recNode.Attributes["LPUPILD"].Value) * 1000.0;
            }
            else
            {
                LeftPupil = -1;
            }


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
