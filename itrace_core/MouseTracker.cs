using System;

namespace iTrace_Core
{
    class MouseTracker : ITracker
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
            s.Show();
        }
    }
}
