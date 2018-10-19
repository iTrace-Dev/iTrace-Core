using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;

namespace iTrace_Core
{
    public partial class CalibrationWindow : Window
    {
        public event EventHandler<CalibrationPointReachedEventArgs> OnCalibrationPointReached;
        public event EventHandler<EventArgs> OnCalibrationFinished;

        private Storyboard storyboard;
        private EllipseGeometry reticle;

        private const String registeredReticleName = "calibrationReticle";
        private const int movementAnimationDurationInMilliseconds = 1500;
        private const int shrinkAnimationDurationInMilliseconds = 500;

        private const double horizontalMargin = 50.0;
        private const double verticalMargin = 200.0;

        private Point currentTarget;

        private Queue<Point> targets;

        public CalibrationWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            PopulateTargets();

            CreateReticle();

            CreateResizeAnimationInStoryboard(10, 2);
            //CreateMovementAnimationInStoryboard(reticle.Center, targets.Dequeue());
        }

        private void PopulateTargets()
        {
            targets = new Queue<Point>();

            double horizontalGap = (this.ActualWidth - (2.0 * horizontalMargin)) / 2.0;
            double verticalGap = (this.ActualHeight - (2.0 * verticalMargin)); 
            
            Point[] points = new Point[]
            {
                new Point(horizontalMargin, verticalMargin),
                new Point(horizontalMargin + horizontalGap, verticalMargin),
                new Point(horizontalMargin + horizontalGap + horizontalGap, verticalMargin),
                new Point(horizontalMargin, verticalMargin + verticalGap),
                new Point(horizontalMargin + horizontalGap, verticalMargin + verticalGap),
                new Point(horizontalMargin + horizontalGap + horizontalGap, verticalMargin + verticalGap),
            };

            ShufflePointArray(points);

            foreach (Point p in points)
            {
                targets.Enqueue(p);
            }
        }

        private void ShufflePointArray(Point[] points)
        {
            Random random = new Random();
            for (int t = 0; t < points.Length; t++)
            {
                Point tmp = points[t];
                int r = random.Next(t, points.Length);
                points[t] = points[r];
                points[r] = tmp;
            }
        }

        private void CreateReticle()
        {
            reticle = new EllipseGeometry();
            reticle.Center = targets.Dequeue();
            reticle.RadiusX = 10;
            reticle.RadiusY = 10;
            this.RegisterName(registeredReticleName, reticle);

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
            currentTarget = to;

            PointAnimation pointAnimation = new PointAnimation();
            pointAnimation.From = from;
            pointAnimation.To = to;
            pointAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(movementAnimationDurationInMilliseconds));

            storyboard = new Storyboard();
            storyboard.Children.Add(pointAnimation);
            Storyboard.SetTargetName(pointAnimation, registeredReticleName);
            Storyboard.SetTargetProperty(pointAnimation, new PropertyPath(EllipseGeometry.CenterProperty));
            storyboard.Completed += new EventHandler(MovementAnimationFinished);
        }

        private void CreateResizeAnimationInStoryboard(double from, double to)
        {
            DoubleAnimation radiusXAnimation = new DoubleAnimation();
            radiusXAnimation.AutoReverse = true;
            radiusXAnimation.From = from;
            radiusXAnimation.To = to;
            radiusXAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(shrinkAnimationDurationInMilliseconds));

            DoubleAnimation radiusYAnimation = new DoubleAnimation();
            radiusYAnimation.AutoReverse = true;
            radiusYAnimation.From = from;
            radiusYAnimation.To = to;
            radiusYAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(shrinkAnimationDurationInMilliseconds));

            storyboard = new Storyboard();

            storyboard.Children.Add(radiusXAnimation);
            Storyboard.SetTargetName(radiusXAnimation, registeredReticleName);
            Storyboard.SetTargetProperty(radiusXAnimation, new PropertyPath(EllipseGeometry.RadiusXProperty));
            
            storyboard.Children.Add(radiusYAnimation);
            Storyboard.SetTargetName(radiusYAnimation, registeredReticleName);
            Storyboard.SetTargetProperty(radiusYAnimation, new PropertyPath(EllipseGeometry.RadiusYProperty));

            storyboard.Completed += new EventHandler(ResizeAnimationFinished);
        }

        private void MovementAnimationFinished(object sender, EventArgs e)
        {
            if (OnCalibrationPointReached != null)
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    OnCalibrationPointReached(this, new CalibrationPointReachedEventArgs(currentTarget));
                }).Start();
            }

            if(!(targets.Count == 0))
            {
                CreateResizeAnimationInStoryboard(10, 1);
                //CreateMovementAnimationInStoryboard(reticle.Center, targets.Dequeue());
                storyboard.Begin(this);
            }
            else
            {
                if(OnCalibrationFinished != null)
                {
                    OnCalibrationFinished(this, new EventArgs());
                }
                this.Close();
            }
        }

        private void ResizeAnimationFinished(object sender, EventArgs e)
        {
            CreateMovementAnimationInStoryboard(reticle.Center, targets.Dequeue());
            storyboard.Begin(this);
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
