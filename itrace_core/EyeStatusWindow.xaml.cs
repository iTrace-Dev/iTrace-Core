using System.Windows;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

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

            leftEyeCircle = new EllipseGeometry(new Point(400,300), 20, 20);
            rightEyeCircle = new EllipseGeometry(new Point(500, 300), 20, 20);

            this.RegisterName(registeredLeftEyeName, leftEyeCircle);
            this.RegisterName(registeredRightEyeName, rightEyeCircle);

            Path leftEyePath = new Path();
            leftEyePath.Fill = Brushes.White;
            leftEyePath.Data = leftEyeCircle;

            Path rightEyePath = new Path();
            rightEyePath.Fill = Brushes.White;
            rightEyePath.Data = rightEyeCircle;

            Canvas containerCanvas = new Canvas();
            containerCanvas.Background = Brushes.Black;
            containerCanvas.Children.Add(leftEyePath);
            containerCanvas.Children.Add(rightEyePath);
            this.Content = containerCanvas;
        }

        public void UpdateEyePosition(Vector3 leftEyePosition, Vector3 rightEyePosition)
        {

        }
    }
}
