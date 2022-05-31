/********************************************************************************************************************************************************
* @file MouseTracker.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

using System;

namespace iTrace_Core
{
    class MouseTracker : ITracker
    {
        private readonly String TRACKER_NAME = "Mouse";
        private const Double TIME_INTERVAL_160 = 6.25;   // 160 samples per second
        private const Double TIME_INTERVAL_125 = 8.0;    // 125 samples per second
        private const Double TIME_INTERVAL_60 = 17.0;   // 60 samples per second
        private System.Timers.Timer MouseLocationTick;

        // for testing
        CalibrationWindow calibrationWindow;

        public MouseTracker()
        {
            MouseLocationTick = new System.Timers.Timer(TIME_INTERVAL_60);
            MouseLocationTick.Elapsed += MousePosition;
            MouseLocationTick.AutoReset = true;
        }

        public String GetTrackerName()
        {
            return TRACKER_NAME;
        }

        public String GetTrackerSerialNumber()
        {
            // Not sure if this would even be useful for the mouse, but the Interface requires it
            return "";
        }

        public bool StartTracker()
        {
            MouseLocationTick.Start();
            return true;
        }

        public void StopTracker()
        {
            MouseLocationTick.Stop();
        }

        /* 
         * While calibration is not necessary with the mouse the tracker
         * interface this class implmenets requires these functions to be
         * present. Here they will be effectively noops but are currently
         * used for testing. 
        */
        public void EnterCalibration()
        {
            calibrationWindow = new CalibrationWindow();
            calibrationWindow.OnCalibrationFinished += CalibrationWindow_OnCalibrationFinished;
            calibrationWindow.Show();
        }

        private void CalibrationWindow_OnCalibrationFinished(object sender, EventArgs e)
        {
            System.Windows.Point[] leftEye = new System.Windows.Point[0];
            System.Windows.Point[] rightEye = new System.Windows.Point[0];

            calibrationWindow.ShowResultsAndClose(leftEye, rightEye);
        }

        public void LeaveCalibration() { }

        private void MousePosition(object sender, EventArgs e)
        {
            System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
            GazeHandler.Instance.EnqueueGaze(new MouseTrackerGazeData(pt.X, pt.Y));
        }

        //Should also be no-op, but currently implemented for testing. 
        public void ShowEyeStatusWindow()
        {
            EyeStatusWindow s = new EyeStatusWindow();
            s.Subscribe();
            s.Show();
        }
    }
}
