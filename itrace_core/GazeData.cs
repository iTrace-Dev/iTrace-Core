using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    public class GazeData
    {
        GazeData()
        {
            //TODO
        }

        // TOBII PRO DATA
        GazeData(Tobii.Research.GazeDataEventArgs tobiiRawGaze)
        {
            //TODO
        }

        // MOUSE TRACKER DATA
        GazeData(int mousePosX, int mousePosY)
        {
            //TODO
        }

        // GAZEPOINT TRACKER DATA
        GazeData(String gazePointRawGaze)
        {
            //TODO
        }
    }
}
