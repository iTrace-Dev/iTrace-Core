using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    interface IGazeData
    {
        String GetEventID();

        String GetLeftX();
        String GetLeftY();
        String GetLeftPupilDiameter();
        String GetLeftValidation();

        String GetRightX();
        String GetRightY();
        String GetRightPupilDiameter();
        String GetRightValidation();

        String GetUserPositionLeftX();
        String GetUserPositionLeftY();
        String GetUserPositionLeftZ();

        String GetUserPositionRightX();
        String GetUserPositionRightY();
        String GetUserPositionRightZ();

    }
}
