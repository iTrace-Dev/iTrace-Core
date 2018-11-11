﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace iTrace_Core
{
    class MouseTracker: ITracker
    {
        private readonly String TRACKER_NAME = "Mouse";
        private const Double TIME_INTERVAL = 8.0; // 125 samples per second
        private System.Timers.Timer MouseLocationTick;

        // for testing
        CalibrationWindow calibrationWindow;

        public MouseTracker()
        {
            MouseLocationTick = new System.Timers.Timer(TIME_INTERVAL);
            MouseLocationTick.Elapsed += MousePosition;
            MouseLocationTick.AutoReset = true;
        }

        public String GetTrackerName()
        {
            return TRACKER_NAME;
        }

        public void StartTracker()
        {
            MouseLocationTick.Start();
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

            calibrationWindow.ShowResultsAndClose();
            
        }

        public void LeaveCalibration() {}

        private void MousePosition(object sender, EventArgs e)
        {
            System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
            //Console.WriteLine(pt);
            GazeHandler.Instance.EnqueueGaze(new GazeData(pt.X, pt.Y));
        }
    }
}
