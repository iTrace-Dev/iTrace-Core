using System;
using System.Windows.Forms;
using System.Xml;

namespace iTrace_Core
{
    public abstract class GazeData
    {
        // Data sent to plugins
        public int X { get; protected set; }
        public int Y { get; protected set; }

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
            X = 0;
            Y = 0;

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
            EventTime = DateTime.UtcNow.Ticks;

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
            /* Useful Data (same applies to right eye):
             *   tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.X
             *   tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.Y
             *   tobiiRawGaze.LeftEye.Pupil.PupilDiameter
             *   tobiiRawGaze.LeftEye.Pupil.Validity
             *   tobiiRawGaze.LeftEye.GazePoint.PositionInUserCoordinates.X // Useful for eye status window
             *   tobiiRawGaze.LeftEye.GazePoint.PositionInUserCoordinates.Y // Useful for eye status window
             *   tobiiRawGaze.LeftEye.GazePoint.PositionInUserCoordinates.Z // Useful for eye status window
             *   tobiiRawGaze.LeftEye.GazePoint.Validity;
            */

            bool isLeftEyeValid = tobiiRawGaze.LeftEye.GazePoint.Validity == Tobii.Research.Validity.Valid;
            bool isRightEyeValid = tobiiRawGaze.RightEye.GazePoint.Validity == Tobii.Research.Validity.Valid;

            if (isLeftEyeValid && isRightEyeValid)
            {
                double avgX = (tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.X + tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.X) / 2.0D;
                double avgY = (tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.Y + tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.Y) / 2.0D;

                X = Convert.ToInt32(avgX * Screen.PrimaryScreen.Bounds.Width);
                Y = Convert.ToInt32(avgY * Screen.PrimaryScreen.Bounds.Height);

            }
            else if(isLeftEyeValid)
            {
                X = Convert.ToInt32(tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.X * Screen.PrimaryScreen.Bounds.Width);
                Y = Convert.ToInt32(tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.Y * Screen.PrimaryScreen.Bounds.Height);
            }
            else if(isRightEyeValid)
            {
                X = Convert.ToInt32(tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.X * Screen.PrimaryScreen.Bounds.Width);
                Y = Convert.ToInt32(tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.Y * Screen.PrimaryScreen.Bounds.Height);
            }
            else
            {
                //Both eyes invalid.
                X = 0;
                Y = 0;
            }

            RightX = tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.X * Screen.PrimaryScreen.Bounds.Width;
            RightY = tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.Y * Screen.PrimaryScreen.Bounds.Height;
            RightPupil = tobiiRawGaze.RightEye.Pupil.PupilDiameter;
            RightValidation = Convert.ToInt32(isRightEyeValid);

            LeftX = tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.X * Screen.PrimaryScreen.Bounds.Width;
            LeftY = tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.Y * Screen.PrimaryScreen.Bounds.Height;
            LeftPupil = tobiiRawGaze.LeftEye.Pupil.PupilDiameter;
            LeftValidation = Convert.ToInt32(isLeftEyeValid);

            UserLeftX = tobiiRawGaze.LeftEye.GazePoint.PositionInUserCoordinates.X;
            UserLeftY = tobiiRawGaze.LeftEye.GazePoint.PositionInUserCoordinates.Y;
            UserLeftZ = tobiiRawGaze.LeftEye.GazePoint.PositionInUserCoordinates.Z;

            UserRightX = tobiiRawGaze.RightEye.GazePoint.PositionInUserCoordinates.X;
            UserRightY = tobiiRawGaze.RightEye.GazePoint.PositionInUserCoordinates.Y;
            UserRightZ = tobiiRawGaze.RightEye.GazePoint.PositionInUserCoordinates.Z;

            //Should be high resolution, but the offset is probably from the .NET epoch (DateTime.MinValue)
            EventTime = DateTime.UtcNow.Ticks;

            TrackerTime = tobiiRawGaze.DeviceTimeStamp;

            SystemTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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

    public class GazepointGazeData : GazeData
    {
        public GazepointGazeData(String gazePointRawGaze) : base()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(gazePointRawGaze);

            XmlNode recNode = xmlDoc.FirstChild;

            if (recNode.Attributes["BPOGX"] == null)
            {
                X = 0;
                Y = 0;
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

            //Should be high resolution, but the offset is probably from the .NET epoch (DateTime.MinValue)
            EventTime = DateTime.UtcNow.Ticks;

            TrackerTime = Convert.ToInt64(recNode.Attributes["TIME_TICK"].Value);

            SystemTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
