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
        private System.Timers.Timer MouseLocaionTick;
        private const Double TIME_INTERVAL = 8.0; // 125 samples per second

        public MouseTracker()
        {
            TrackerName = "Mouse";
            MouseLocaionTick = new System.Timers.Timer(TIME_INTERVAL);
            MouseLocaionTick.Elapsed += MousePosition;
            MouseLocaionTick.AutoReset = true;
        }

        public String GetTrackerName()
        {
            return TrackerName;
        }

        public void StartTracker()
        {
            MouseLocaionTick.Start();
        }

        public void StopTracker()
        {
            MouseLocaionTick.Stop();
        }

        public void EnterCalibration()
        {
            //TODO
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
