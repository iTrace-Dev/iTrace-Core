using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iTrace_Core
{
    public abstract class GazeData
    {
        protected String foobar;

        public int X { get; protected set; }
        public int Y { get; protected set; }

        public bool IsEmpty()
        {
            return foobar == "";
        }

        public String Output()
        {
            return foobar;
        }
    }

    public class TobiiGazeData : GazeData
    {
        public TobiiGazeData(Tobii.Research.GazeDataEventArgs tobiiRawGaze)
        {
            //TODO
            foobar = "Left: " + tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.X + " " + tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.Y + " " +
            tobiiRawGaze.LeftEye.Pupil.PupilDiameter + " " + tobiiRawGaze.LeftEye.Pupil.Validity + " " + tobiiRawGaze.LeftEye.GazePoint.PositionInUserCoordinates.Z + " " +
            tobiiRawGaze.LeftEye.GazePoint.Validity;

            X = Convert.ToInt32(tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.X * Screen.PrimaryScreen.Bounds.X);
            Y = Convert.ToInt32(tobiiRawGaze.RightEye.GazePoint.PositionOnDisplayArea.Y * Screen.PrimaryScreen.Bounds.Y);
        }
    }

    public class MouseTrackerGazeData : GazeData
    {
        public MouseTrackerGazeData(int mousePosX, int mousePosY)
        {
            foobar = "Mouse: " + mousePosX + " " + mousePosY;

            X = mousePosX;
            Y = mousePosY;
        }
    }

    public class GazepointGazeData : GazeData
    {
        public GazepointGazeData(String gazePointRawGaze)
        {
            foobar = gazePointRawGaze;

            //TODO: initialize X and Y
        }
    }

    public class EmptyGazeData : GazeData
    {
        public EmptyGazeData()
        {
            foobar = "";
        }
    }
}
