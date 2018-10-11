using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace iTrace_Core
{
    public partial class CalibrationWindow : Window
    {
        public event EventHandler<CalibrationPointReachedEventArgs> OnCalibrationPointReached;

        private Storyboard storyboard;
        private EllipseGeometry reticle;
        private static String reticleRegisteredName = "calibrationReticle";

        public CalibrationWindow()
        {
            InitializeComponent();
            CreateReticle();
            CreateMovementAnimationInStoryboard(new Point(100, 100), new Point(1920, 1080));
        }

        private void CreateReticle()
        {
            reticle = new EllipseGeometry();
            reticle.Center = new Point(100, 100);
            reticle.RadiusX = 10;
            reticle.RadiusY = 10;
            this.RegisterName(reticleRegisteredName, reticle);

            Path myPath = new Path();
            myPath.Fill = Brushes.Blue;
            myPath.Data = reticle;
            myPath.Loaded += ReticleLoaded;

            Canvas containerCanvas = new Canvas();
            containerCanvas.Children.Add(myPath);
            Content = containerCanvas;
        }

        private void ReticleLoaded(object sender, RoutedEventArgs e)
        {
            storyboard.Begin(this);
        }

        private void CreateMovementAnimationInStoryboard(Point from, Point to)
        {
            PointAnimation myDoubleAnimation = new PointAnimation();
            myDoubleAnimation.From = from;
            myDoubleAnimation.To = to;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(5));

            storyboard = new Storyboard();
            storyboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetName(myDoubleAnimation, reticleRegisteredName);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(EllipseGeometry.CenterProperty));
            storyboard.Completed += new EventHandler(AnimationFinished);
        }

        private void AnimationFinished(object sender, EventArgs e)
        {
            if (OnCalibrationPointReached != null)
            {
                OnCalibrationPointReached(this, new CalibrationPointReachedEventArgs(new Point()));
            }
            this.Close();
        }

    }

    public class CalibrationPointReachedEventArgs : EventArgs
    {
        public Point CalibrationPoint { get; private set; }

        public CalibrationPointReachedEventArgs(Point point)
        {
            CalibrationPoint = point;
        }
    }
}
