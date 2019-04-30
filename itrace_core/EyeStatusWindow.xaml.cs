using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace iTrace_Core
{
    public partial class EyeStatusWindow : Window
    {
        const string registeredLeftEyeName = "leftEyeCircle";
        const string registeredRightEyeName = "rightEyeCircle";

        EllipseGeometry leftEyeCircle;
        EllipseGeometry rightEyeCircle;

        const double maxDistCentimeter = 100.0;

        public EyeStatusWindow()
        {
            InitializeComponent();

            leftEyeCircle = new EllipseGeometry(new Point(150, 100), 5, 5);
            rightEyeCircle = new EllipseGeometry(new Point(250, 100), 5, 5);

            this.RegisterName(registeredLeftEyeName, leftEyeCircle);
            this.RegisterName(registeredRightEyeName, rightEyeCircle);

            Path leftEyePath = new Path();
            leftEyePath.Fill = Brushes.White;
            leftEyePath.Data = leftEyeCircle;

            Path rightEyePath = new Path();
            rightEyePath.Fill = Brushes.White;
            rightEyePath.Data = rightEyeCircle;
            
            DrawDestination.Background = Brushes.Black;
            DrawDestination.Children.Add(leftEyePath);
            DrawDestination.Children.Add(rightEyePath);
        }

        public void Subscribe()
        {
            GazeHandler.Instance.OnGazeDataReceived += ReceiveGazeData;
        }

        public void Unsubscribe()
        {
            GazeHandler.Instance.OnGazeDataReceived -= ReceiveGazeData;
        }

        private void ReceiveGazeData(object sender, GazeDataReceivedEventArgs e)
        {
            if (e.ReceivedGazeData.IsValid())
            {
                Vector3 leftEyePosition = new Vector3(Convert.ToSingle(e.ReceivedGazeData.UserLeftX),
                                                      Convert.ToSingle(e.ReceivedGazeData.UserLeftY),
                                                      Convert.ToSingle(e.ReceivedGazeData.UserLeftZ));


                Vector3 rightEyePosition = new Vector3(Convert.ToSingle(e.ReceivedGazeData.UserRightX),
                                                       Convert.ToSingle(e.ReceivedGazeData.UserRightY),
                                                       Convert.ToSingle(e.ReceivedGazeData.UserRightZ));

                UpdateEyePosition(leftEyePosition, rightEyePosition);
            }
        }

        Vector3 HomogeneousTo3D(Vector4 vec)
        {
            Vector3 result;

            result.X = vec.X / vec.W;
            result.Y = vec.Y / vec.W;
            result.Z = vec.Z / vec.W;

            return result;
        }

        double ClampedLerp(double origin, double destination, double percent)
        {
            if (percent > 1.0)
                percent = 1.0;
            else if (percent < 0)
                percent = 0.0;

            return origin * (1 - percent) + destination * percent;
        }

        public void UpdateEyePosition(Vector3 leftEyePosition, Vector3 rightEyePosition)
        {
            Vector4 leftEyeHomogeneous = new Vector4(leftEyePosition, leftEyePosition.Z / 1000f);
            Vector4 rightEyeHomogeneous = new Vector4(rightEyePosition, rightEyePosition.Z / 1000f);

            Vector4 reflect = new Vector4(1, -1, 1, 1);
            leftEyeHomogeneous = leftEyeHomogeneous * reflect;
            rightEyeHomogeneous = rightEyeHomogeneous * reflect;

            Vector3 leftEyeProjected = HomogeneousTo3D(leftEyeHomogeneous);
            Vector3 rightEyeProjected = HomogeneousTo3D(rightEyeHomogeneous);

            float avgZ = 0.0f;

            if(Single.IsNaN(leftEyePosition.Z))
            {
                avgZ = rightEyePosition.Z;
            }
            else if (Single.IsNaN(rightEyePosition.Z))
            {
                avgZ = leftEyePosition.Z;
            }
            else
            {
                avgZ = (leftEyePosition.Z + rightEyePosition.Z) / 2.0f;
            }

            int distanceInCentimeters = Convert.ToInt32(avgZ / 10.0f); //Divided by 10 to convert to cm

            Dispatcher.Invoke(
                () =>
                {
                    leftEyeCircle.Center = new Point(leftEyeProjected.X + (DrawDestination.ActualWidth / 2), leftEyeProjected.Y + (DrawDestination.ActualHeight / 2));
                    rightEyeCircle.Center = new Point(rightEyeProjected.X + (DrawDestination.ActualWidth / 2), rightEyeProjected.Y + (DrawDestination.ActualHeight / 2));

                    Slider.Content = "◁ " + distanceInCentimeters.ToString() + " cm";

                    Thickness margin = Slider.Margin;
                    margin.Top = ClampedLerp(0.0, ActualHeight - 65.0, distanceInCentimeters / maxDistCentimeter);
                    Slider.Margin = margin;
                });
        }
    }
}
