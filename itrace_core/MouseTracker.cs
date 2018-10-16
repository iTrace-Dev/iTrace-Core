using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    class MouseTracker: ITracker
    {
        private readonly String TrackerName;
        private System.Timers.Timer MouseLocationTick;
        private const Double TIME_INTERVAL = 8.0; // 125 samples per second

        CalibrationWindow calibrationWindow;

        public MouseTracker()
        {
            TrackerName = "Mouse";
            MouseLocationTick = new System.Timers.Timer(TIME_INTERVAL);
            MouseLocationTick.Elapsed += MousePosition;
            MouseLocationTick.AutoReset = true;
        }

        public String GetTrackerName()
        {
            return TrackerName;
        }

        public void StartTracker()
        {
            MouseLocationTick.Start();
        }

        public void StopTracker()
        {
            MouseLocationTick.Stop();
        }

        public void EnterCalibration()
        {
            //For testing
            calibrationWindow = new CalibrationWindow();
            calibrationWindow.Show();
        }

        public void LeaveCalibration()
        {
            //TODO
        }

        private void MousePosition(object sender, EventArgs e)
        {
            System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
            Console.WriteLine(pt);
        }
    }
}
