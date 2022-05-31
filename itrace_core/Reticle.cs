using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace iTrace_Core
{
    public class Reticle : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_LAYERED = 0x80000;
        const int WS_EX_TRANSPARENT = 0x20;

        Pen crossPen = new Pen(Color.Red, 3);
        private int maximumNumberOfPoints = 15;
        private int totalX;
        private int totalY;
        private List<int> xPoints = new List<int>();
        private List<int> yPoints = new List<int>();
        private bool display;
        public static Point newPos;

        public Reticle()
        {
            Hide();
            totalX = 0;
            totalY = 0;
            display = false;
            newPos = new Point(0, 0);

            TopMost = true;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.LightGreen;
            TransparencyKey = Color.LightGreen;

            Width = 60;
            Height = 60;

            Paint += new PaintEventHandler(ReticleFormPaint);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var existingStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, existingStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }

        void ReticleFormPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawEllipse(crossPen, (Width - 15) / 2, (Height - 15) / 2, 15, 15);
        }

        public void ToDraw(bool draw)
        {
            display = draw;
            if (display)
                Show();
            else
                Hide();
        }

        public void UpdateReticle(int x, int y)
        {
            //No reason to do anything if it can't be seen...
            if (!display)
                return;

            //AL: This check needs redone, negative coordinates are possible when using multiple screens
            //A similar check can be done by consulting the Screen class

            //// Invalid screen coordinates...
            //if (x < 0 || y < 0)
            //    return;

            //Sum up all the x and y we have seen
            totalX += x;
            totalY += y;

            //Add them to the list of values we have seen
            xPoints.Insert(0, x);
            yPoints.Insert(0, y);

            /*
             * If we have enough points (MAX_NUM_POINTS) for desired smoothness
             * then we take an average of the points and move the reticle.
             *
             * To save re-totaling the points, we then just remove the oldest value from the
             * total and the lists (last) so the next time the function is called we only
             * evaluate the last MAX_NUM_POINTS.
             */

            if (xPoints.Count == maximumNumberOfPoints)
            {
                double avgX = totalX / maximumNumberOfPoints;
                double avgY = totalY / maximumNumberOfPoints;

                newPos = new Point(Convert.ToInt32(avgX), Convert.ToInt32(avgY));
                newPos.Offset(-(Width / 2), -(Height / 2));

                //Remove oldest points from totals and lists
                totalX -= xPoints.Last();
                totalY -= yPoints.Last();
                xPoints.RemoveAt(xPoints.Count - 1);
                yPoints.RemoveAt(yPoints.Count - 1);

                // Update the Reticle Location without Timer
                this.Invoke((MethodInvoker)delegate
                {
                    Location = newPos;
                });
            }
        }

        public void CompleteEvents()
        {
            Application.DoEvents();
        }
    }
}
