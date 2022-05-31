/********************************************************************************************************************************************************
* @file CalibrationWindow.xaml.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using iTrace_Core.Properties;

namespace iTrace_Core
{
    public partial class CalibrationWindow : Window
    {
        private enum AnimationStates
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

        private const double horizontalMargin = 150.0;
        private const double verticalMargin = 150.0;

        private Point[] targets;
        private int currentTargetIndex;

        bool pressKeyToClose = false;

        private AnimationStates currentAnimationState;

        public CalibrationWindow()
        {
            InitializeComponent();

            System.Windows.Forms.Screen calibrationMonitor = System.Windows.Forms.Screen.AllScreens[Settings.Default.calibration_monitor];

            Left = calibrationMonitor.Bounds.Left;
            Top = calibrationMonitor.Bounds.Top;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;

            GenerateTargets();

            CreateReticle();

            currentAnimationState = AnimationStates.Appear;
        }

        private void ReticleLoaded(object sender, RoutedEventArgs e)
        {
            RunCalibration();
        }

        private void RunCalibration()
        {
            switch (currentAnimationState)
            {
                case AnimationStates.Appear:
                    currentAnimationState = AnimationStates.Shrink;

                    CreateResizeAnimationInStoryboard(GetCurrentReticleRadius(), beginningGrownReticleSize);
                    RunStoryboard();
                    break;

                case AnimationStates.FinishedCalibrationCallback:
                    if (OnCalibrationFinished != null)
                    {
                        OnCalibrationFinished(this, new EventArgs());
                    }
                    break;

                case AnimationStates.Grow:
                    currentAnimationState = AnimationStates.Move;

                    if (currentTargetIndex + 1 == targets.Length)
                    {
                        currentAnimationState = AnimationStates.FinishedCalibrationCallback;
                    }

                    CreateResizeAnimationInStoryboard(GetCurrentReticleRadius(), defaultReticleRadius);
                    RunStoryboard();
                    break;

                case AnimationStates.Move:
                    currentAnimationState = AnimationStates.Shrink;
                    ++currentTargetIndex;

                    CreateMovementAnimationInStoryboard(reticle.Center, targets[currentTargetIndex]);
                    RunStoryboard();
                    break;

                case AnimationStates.CalibrationPointReachedCallback:
                    currentAnimationState = AnimationStates.Grow;

                    if (OnCalibrationPointReached != null)
                    {
                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;
                            OnCalibrationPointReached(this, new CalibrationPointReachedEventArgs(targets[currentTargetIndex]));
                        }).Start();
                    }
                    RunCalibration();
                    break;

                case AnimationStates.Shrink:
                    currentAnimationState = AnimationStates.CalibrationPointReachedCallback;

                    CreateResizeAnimationInStoryboard(GetCurrentReticleRadius(), shrunkenReticleRadius);
                    RunStoryboard();
                    break;
            }
        }

        private void GenerateTargets()
        {
            double horizontalGap = (this.ActualWidth - (2.0 * horizontalMargin)) / 2.0;
            double verticalGap = (this.ActualHeight - (2.0 * verticalMargin)) / 2.0;

            targets = new Point[]
            {
                new Point(horizontalMargin, verticalMargin),
                new Point(horizontalMargin + horizontalGap, verticalMargin),
                new Point(horizontalMargin + horizontalGap + horizontalGap, verticalMargin),

                new Point(horizontalMargin, verticalMargin + verticalGap),
                new Point(horizontalMargin + horizontalGap, verticalMargin + verticalGap),
                new Point(horizontalMargin + horizontalGap + horizontalGap, verticalMargin + verticalGap),

                new Point(horizontalMargin, verticalMargin + verticalGap + verticalGap),
                new Point(horizontalMargin + horizontalGap, verticalMargin + verticalGap + verticalGap),
                new Point(horizontalMargin + horizontalGap + horizontalGap, verticalMargin + verticalGap + verticalGap)
            };

            ShufflePointArray(targets);
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
            reticle.Center = targets[0];
            reticle.RadiusX = defaultReticleRadius;
            reticle.RadiusY = defaultReticleRadius;
            this.RegisterName(registeredReticleName, reticle);

            Path myPath = new Path();
            myPath.Fill = Brushes.Blue;
            myPath.Data = reticle;
            myPath.Loaded += ReticleLoaded;

            Canvas containerCanvas = new Canvas();
            containerCanvas.Children.Add(myPath);
            this.Content = containerCanvas;
        }

        private void RunStoryboard()
        {
            storyboard.Completed += new EventHandler(ContinueCalibrationCallback);
            storyboard.Begin(this);
        }

        private void ContinueCalibrationCallback(object sender, EventArgs e)
        {
            RunCalibration();
        }

        private double GetCurrentReticleRadius()
        {
            return reticle.RadiusX;
        }

        private void CreateMovementAnimationInStoryboard(Point from, Point to)
        {
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
            Grid grid = new Grid();
            
            Canvas containerCanvas = new Canvas();


            EllipseGeometry[] targetEllipses = new EllipseGeometry[targets.Length];
            Path[] targetPaths = new Path[targets.Length];

            for(int i = 0; i < targets.Length; ++i)
            {
                targetEllipses[i] = new EllipseGeometry();
                targetEllipses[i].RadiusX = 20;
                targetEllipses[i].RadiusY = 20;
                targetEllipses[i].Center = targets[i];

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

            Label finishedLabel = new Label();
            finishedLabel.Content = "Press any key to continue...";
            finishedLabel.FontSize = 25;
            finishedLabel.HorizontalAlignment = HorizontalAlignment.Right;
            finishedLabel.VerticalAlignment = VerticalAlignment.Bottom;
            finishedLabel.Margin = new Thickness(0.0, 0.0, 10.0, 20.0);

            grid.Children.Add(finishedLabel);
            grid.Children.Add(containerCanvas);
            this.Content = grid;

            pressKeyToClose = true;
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (pressKeyToClose)
            {
                this.Close();
            }
        }

        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (pressKeyToClose)
            {
                this.Close();
            }
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
