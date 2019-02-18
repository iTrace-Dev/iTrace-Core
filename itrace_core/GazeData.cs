using System;
using System.Windows.Forms;
using System.Xml;

namespace iTrace_Core
{
    public abstract class GazeData
    {
        protected String foobar;

        public int X { get; protected set; }
        public int Y { get; protected set; }

        public long Timestamp { get; protected set; }

        public GazeData()
        {
            X = 0;
            Y = 0;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public bool IsEmpty()
        {
            return foobar == "";
        }

        public String Output()
        {
            return foobar;
        }

        public String Serialize()
        {
            return "gaze," + Timestamp.ToString() + "," + X.ToString() + "," + Y.ToString() + "\n";
        }
    }

    public class TobiiGazeData : GazeData
    {
        public TobiiGazeData(Tobii.Research.GazeDataEventArgs tobiiRawGaze) : base()
        {
            //TODO
            foobar = "Left: " + tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.X + " " + tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.Y + " " +
            tobiiRawGaze.LeftEye.Pupil.PupilDiameter + " " + tobiiRawGaze.LeftEye.Pupil.Validity + " " + tobiiRawGaze.LeftEye.GazePoint.PositionInUserCoordinates.Z + " " +
            tobiiRawGaze.LeftEye.GazePoint.Validity;

            bool isLeftEyeValid = tobiiRawGaze.LeftEye.GazePoint.Validity == Tobii.Research.Validity.Valid;
            bool isRightEyeValid = tobiiRawGaze.RightEye.GazePoint.Validity == Tobii.Research.Validity.Valid;

            if (isLeftEyeValid && isRightEyeValid)
            {
                float avgX = (tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.X + tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.X) / 2.0f;
                float avgY = (tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.Y + tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.Y) / 2.0f;

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
                //NaN can't be marked with an integer, should probably move to using a float

                //Todo: probably store validity in GazeData
                X = 0;
                Y = 0;
            }

            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    public class MouseTrackerGazeData : GazeData
    {
        public MouseTrackerGazeData(int mousePosX, int mousePosY) : base()
        {
            foobar = "Mouse: " + mousePosX + " " + mousePosY;

            X = mousePosX;
            Y = mousePosY;
        }
    }

    public class GazepointGazeData : GazeData
    {
        public GazepointGazeData(String gazePointRawGaze) : base()
        {
            foobar = gazePointRawGaze;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(gazePointRawGaze);

            XmlNode recNode = xmlDoc.FirstChild;
            if (recNode.Attributes["BPOGX"] == null)
            {
                X = 0;
                Y = 0;
                return;
            }

            X = Convert.ToInt32(float.Parse(recNode.Attributes["BPOGX"].Value) * Screen.PrimaryScreen.Bounds.Width);
            Y = Convert.ToInt32(float.Parse(recNode.Attributes["BPOGY"].Value) * Screen.PrimaryScreen.Bounds.Height);
        }
    }

    public class EmptyGazeData : GazeData
    {
        public EmptyGazeData() : base()
        {
            foobar = "";
        }
    }
}
