using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace iTrace_Core
{
    public partial class CalibrationWindow : Window
    {
        enum AnimationStates
        {
            Appear,
            Move,
            Shrink,
            Grow,
            CalibrationPointReachedCallback,
            FinishedCalibrationCallback
        }

        public event EventHandler<CalibrationPointReachedEventArgs> OnCalibrationPointReached;
        public event EventHandler<EventArgs> OnCalibrationFinished;

        private Storyboard storyboard;
        private EllipseGeometry reticle;

        private const String registeredReticleName = "calibrationReticle";

        private const int movementAnimationDurationInMilliseconds = 2000;
        private const int resizeAnimationDurationInMilliseconds = 700;

        private const int defaultReticleRadius = 10;
        private const int shrunkenReticleRadius = 1;
        private const int beginningGrownReticleSize = 20;

        private const double horizontalMargin = 200.0;
        private const double verticalMargin = 200.0;

        private readonly TimeSpan closeCountDownTimeSpan = new TimeSpan(0, 0, 5);

        private Point currentTarget;

        Point[] points;
        private Queue<Point> targets;
        private Queue<AnimationStates> windowStateQueue;

        System.Windows.Threading.DispatcherTimer closeWindowTimer;

        public CalibrationWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            GenerateTargets();
            GenerateStates();

            CreateReticle();

            closeWindowTimer = new System.Windows.Threading.DispatcherTimer();
            closeWindowTimer.Interval = closeCountDownTimeSpan;
            closeWindowTimer.Tick += CloseWindowTimer_Tick;
        }

        private void ReticleLoaded(object sender, RoutedEventArgs e)
        {
            RunCalibration();
        }

        private void RunCalibration()
        {
            if (windowStateQueue.Count < 1)
                return;

            AnimationStates currentAnimationState = windowStateQueue.Dequeue();

            switch (currentAnimationState)
            {
                case AnimationStates.Appear:
                    CreateResizeAnimationInStoryboard(GetCurrentReticleRadius(), beginningGrownReticleSize);
                    storyboard.Completed += new EventHandler(ContinueCalibrationCallback);
                    storyboard.Begin(this);
                    break;

                case AnimationStates.FinishedCalibrationCallback:
                    if (OnCalibrationFinished != null)
                    {
                        OnCalibrationFinished(this, new EventArgs());
                    }
                    break;

                case AnimationStates.Grow:
                    CreateResizeAnimationInStoryboard(GetCurrentReticleRadius(), defaultReticleRadius);
                    storyboard.Completed += new EventHandler(ContinueCalibrationCallback);
                    storyboard.Begin(this);
                    break;

                case AnimationStates.Move:
                    CreateMovementAnimationInStoryboard(reticle.Center, targets.Dequeue());
                    storyboard.Completed += new EventHandler(ContinueCalibrationCallback);
                    storyboard.Begin(this);
                    break;

                case AnimationStates.CalibrationPointReachedCallback:
                    if (OnCalibrationPointReached != null)
                    {
                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;
                            OnCalibrationPointReached(this, new CalibrationPointReachedEventArgs(currentTarget));
                        }).Start();
                    }
                    RunCalibration();
                    break;

                case AnimationStates.Shrink:
                    CreateResizeAnimationInStoryboard(GetCurrentReticleRadius(), shrunkenReticleRadius);
                    storyboard.Completed += new EventHandler(ContinueCalibrationCallback);
                    storyboard.Begin(this);
                    break;
            }
        }

        private void ContinueCalibrationCallback(object sender, EventArgs e)
        {
            RunCalibration();
        }

        private double GetCurrentReticleRadius()
        {
            return reticle.RadiusX;
        }

        private void GenerateStates()
        {
            windowStateQueue = new Queue<AnimationStates>();

            windowStateQueue.Enqueue(AnimationStates.Appear);
            windowStateQueue.Enqueue(AnimationStates.Shrink);
            windowStateQueue.Enqueue(AnimationStates.CalibrationPointReachedCallback);
            windowStateQueue.Enqueue(AnimationStates.Grow);

            for (int i = 1; i < targets.Count; ++i)
            {
                windowStateQueue.Enqueue(AnimationStates.Move);
                windowStateQueue.Enqueue(AnimationStates.Shrink);
                windowStateQueue.Enqueue(AnimationStates.CalibrationPointReachedCallback);
                windowStateQueue.Enqueue(AnimationStates.Grow);
            }

            windowStateQueue.Enqueue(AnimationStates.FinishedCalibrationCallback);
        }

        private void GenerateTargets()
        {
            targets = new Queue<Point>();

            double horizontalGap = (this.ActualWidth - (2.0 * horizontalMargin)) / 2.0;
            double verticalGap = (this.ActualHeight - (2.0 * verticalMargin));

            points = new Point[]
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
            reticle.RadiusX = defaultReticleRadius;
            reticle.RadiusY = defaultReticleRadius;
            this.RegisterName(registeredReticleName, reticle);

            Path myPath = new Path();
            myPath.Fill = Brushes.Blue;
            myPath.Data = reticle;
            myPath.Loaded += ReticleLoaded;

            Canvas containerCanvas = new Canvas();
            containerCanvas.Children.Add(myPath);
            Content = containerCanvas;
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
        }

        private void CreateResizeAnimationInStoryboard(double from, double to)
        {
            DoubleAnimation radiusXAnimation = new DoubleAnimation();
            radiusXAnimation.From = from;
            radiusXAnimation.To = to;
            radiusXAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(resizeAnimationDurationInMilliseconds));

            DoubleAnimation radiusYAnimation = new DoubleAnimation();
            radiusYAnimation.From = from;
            radiusYAnimation.To = to;
            radiusYAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(resizeAnimationDurationInMilliseconds));

            storyboard = new Storyboard();

            storyboard.Children.Add(radiusXAnimation);
            Storyboard.SetTargetName(radiusXAnimation, registeredReticleName);
            Storyboard.SetTargetProperty(radiusXAnimation, new PropertyPath(EllipseGeometry.RadiusXProperty));

            storyboard.Children.Add(radiusYAnimation);
            Storyboard.SetTargetName(radiusYAnimation, registeredReticleName);
            Storyboard.SetTargetProperty(radiusYAnimation, new PropertyPath(EllipseGeometry.RadiusYProperty));
        }

        public void ShowResultsAndClose(Point[] leftEyePoints, Point[] rightEyePoints)
        {
            Canvas containerCanvas = new Canvas();


            EllipseGeometry[] targetEllipses = new EllipseGeometry[points.Length];
            Path[] targetPaths = new Path[points.Length];

            for(int i = 0; i < points.Length; ++i)
            {
                targetEllipses[i] = new EllipseGeometry();
                targetEllipses[i].RadiusX = 20;
                targetEllipses[i].RadiusY = 20;
                targetEllipses[i].Center = points[i];

                targetPaths[i] = new Path();
                targetPaths[i].StrokeThickness = 2;
                targetPaths[i].Stroke = Brushes.Blue;
                targetPaths[i].Fill = Brushes.Transparent;
                targetPaths[i].Data = targetEllipses[i];
                containerCanvas.Children.Add(targetPaths[i]);
            }

            EllipseGeometry[] leftEyeEllipses = new EllipseGeometry[leftEyePoints.Length];
            Path[] leftEyePaths = new Path[leftEyePoints.Length];

            for (int i = 0; i < leftEyePoints.Length; ++i)
            {
                leftEyeEllipses[i] = new EllipseGeometry();
                leftEyeEllipses[i].RadiusX = 3;
                leftEyeEllipses[i].RadiusY = 3;
                leftEyeEllipses[i].Center = leftEyePoints[i];

                leftEyePaths[i] = new Path();
                leftEyePaths[i].Fill = Brushes.Red;
                leftEyePaths[i].Data = leftEyeEllipses[i];
                containerCanvas.Children.Add(leftEyePaths[i]);
            }

            EllipseGeometry[] rightEyeEllipses = new EllipseGeometry[rightEyePoints.Length];
            Path[] rightEyePaths = new Path[rightEyePoints.Length];

            for (int i = 0; i < rightEyePoints.Length; ++i)
            {
                rightEyeEllipses[i] = new EllipseGeometry();
                rightEyeEllipses[i].RadiusX = 3;
                rightEyeEllipses[i].RadiusY = 3;
                rightEyeEllipses[i].Center = rightEyePoints[i];

                rightEyePaths[i] = new Path();
                rightEyePaths[i].Fill = Brushes.Blue;
                rightEyePaths[i].Data = rightEyeEllipses[i];
                containerCanvas.Children.Add(rightEyePaths[i]);
            }

            Content = containerCanvas;

            closeWindowTimer.Start();
        }

        private void CloseWindowTimer_Tick(object sender, EventArgs e)
        {
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
