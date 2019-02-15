﻿using System;

namespace iTrace_Core
{
    interface ITracker
    {
        void EnterCalibration();
        void LeaveCalibration();
        void StartTracker();
        void StopTracker();
        String GetTrackerName();
        void ShowEyeStatusWindow();
    }
}
