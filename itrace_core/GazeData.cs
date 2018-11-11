using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    public abstract class GazeData
    {
        protected String foobar;

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
        }
    }

    public class MouseTrackerGazeData : GazeData
    {
        public MouseTrackerGazeData(int mousePosX, int mousePosY)
        {
            foobar = "Mouse: " + mousePosX + " " + mousePosY;
        }
    }

    public class GazepointGazeData : GazeData
    {
        public GazepointGazeData(String gazePointRawGaze)
        {
            foobar = gazePointRawGaze;
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
