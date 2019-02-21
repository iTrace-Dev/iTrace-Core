using System.Windows;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System;

namespace iTrace_Core
{
    public partial class EyeStatusWindow : Window
    {
        const string registeredLeftEyeName = "leftEyeCircle";
        const string registeredRightEyeName = "rightEyeCircle";

        EllipseGeometry leftEyeCircle;
        EllipseGeometry rightEyeCircle;

        public EyeStatusWindow()
        {
            InitializeComponent();

            leftEyeCircle = new EllipseGeometry(new Point(400,300), 5, 5);
            rightEyeCircle = new EllipseGeometry(new Point(500, 300), 5, 5);

            this.RegisterName(registeredLeftEyeName, leftEyeCircle);
            this.RegisterName(registeredRightEyeName, rightEyeCircle);

            Path leftEyePath = new Path();
            leftEyePath.Fill = Brushes.White;
            leftEyePath.Data = leftEyeCircle;

            Path rightEyePath = new Path();
            rightEyePath.Fill = Brushes.White;
            rightEyePath.Data = rightEyeCircle;

            this.KeyUp += Grid_KeyUp;
            Canvas containerCanvas = new Canvas();
            containerCanvas.Background = Brushes.Black;
            containerCanvas.Children.Add(leftEyePath);
            containerCanvas.Children.Add(rightEyePath);
            this.Content = containerCanvas;
        }

        Vector3 HomogeneousTo3D(Vector4 vec)
        {
            Vector3 result;

            result.X = vec.X / vec.W;
            result.Y = vec.Y / vec.W;
            result.Z = vec.Z / vec.W;

            return result;
        }

        public void UpdateEyePosition(Vector3 leftEyePosition, Vector3 rightEyePosition)
        {
            Vector4 leftEyeHomogeneous = new Vector4(leftEyePosition, leftEyePosition.Z / 10);
            Vector4 rightEyeHomogeneous = new Vector4(rightEyePosition, rightEyePosition.Z / 10);

            Vector3 leftEyeProjected = HomogeneousTo3D(leftEyeHomogeneous);
            Vector3 rightEyeProjected = HomogeneousTo3D(rightEyeHomogeneous);

            leftEyeCircle.Center = new Point(leftEyeProjected.X + (Width / 2), leftEyeProjected.Y + (Height / 2));
            rightEyeCircle.Center = new Point(rightEyeProjected.X + (Width / 2), rightEyeProjected.Y + (Height / 2));
        }

        //Temporary for testing
        Vector3 currentLeftEyePosition = new Vector3(10.0f, 0.0f, 5.0f);
        Vector3 currentRightEyePosition = new Vector3(-10.0f, 0.0f, 5.0f);

        private void Grid_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.W)
            {
                currentRightEyePosition.Z += 1.0f;
                currentLeftEyePosition.Z += 1.0f;
            }
            else if (e.Key == System.Windows.Input.Key.S)
            {
                currentRightEyePosition.Z -= 1.0f;
                currentLeftEyePosition.Z -= 1.0f;
            }
            else if (e.Key == System.Windows.Input.Key.A)
            {
                currentRightEyePosition.X += 10.0f;
                currentLeftEyePosition.X += 10.0f;
            }
            else if (e.Key == System.Windows.Input.Key.D)
            {
                currentRightEyePosition.X -= 10.0f;
                currentLeftEyePosition.X -= 10.0f;
            }
            else if (e.Key == System.Windows.Input.Key.R)
            {
                currentRightEyePosition.Y += 10.0f;
                currentLeftEyePosition.Y += 10.0f;
            }
            else if (e.Key == System.Windows.Input.Key.F)
            {
                currentRightEyePosition.Y -= 10.0f;
                currentLeftEyePosition.Y -= 10.0f;
            }

            Console.WriteLine(currentLeftEyePosition.ToString());

            UpdateEyePosition(currentLeftEyePosition, currentRightEyePosition);
        }
    }
}
