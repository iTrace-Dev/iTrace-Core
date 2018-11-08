using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    public class GazeData
    {
        String foobar;
        public GazeData()
        {
            foobar = "";
        }

        // TOBII PRO DATA
        public GazeData(Tobii.Research.GazeDataEventArgs tobiiRawGaze)
        {
            //TODO
            foobar = "Left: " + tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.X + " " + tobiiRawGaze.LeftEye.GazePoint.PositionOnDisplayArea.Y + " " +
            tobiiRawGaze.LeftEye.Pupil.PupilDiameter + " " + tobiiRawGaze.LeftEye.Pupil.Validity + " " + tobiiRawGaze.LeftEye.GazePoint.PositionInUserCoordinates.Z + " " +
            tobiiRawGaze.LeftEye.GazePoint.Validity;
        }

        // MOUSE TRACKER DATA
        public GazeData(int mousePosX, int mousePosY)
        {
            foobar = "Mouse: " + mousePosX + " " + mousePosY;
        }

        // GAZEPOINT TRACKER DATA
        public GazeData(String gazePointRawGaze)
        {
            foobar = gazePointRawGaze;
        }

        public bool IsEmpty()
        {
            return foobar == "";
        }

        public String Output()
        {
            return foobar;
        }
    }
}
