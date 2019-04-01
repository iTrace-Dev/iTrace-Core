using System;
using System.ComponentModel;

namespace iTrace_Core
{
    class TobiiTracker : ITracker
    {
        private readonly Tobii.Research.IEyeTracker TrackingDevice;
        private Tobii.Research.ScreenBasedCalibration Calibration;
        private CalibrationWindow calibrationWindow;

        private EyeStatusWindow eyeStatusWindow;
        private bool isEyeStatusOpen;
        private Single previousTrackingSpeed = 0;

        private readonly Single MAXIMUM_TRACKING_SPEED = 60;
        private static long ThrottleSkip = 0;
        private static long SkipCount = 0;


        public TobiiTracker(Tobii.Research.IEyeTracker foundDevice)
        {
            TrackingDevice = foundDevice;
            isEyeStatusOpen = false;
            
            // Check if device supports 60Hz
            foreach (Single x in foundDevice.GetAllGazeOutputFrequencies())
            {
                if (x == MAXIMUM_TRACKING_SPEED)
                {
                    // Cache the old speed to restore later
                    previousTrackingSpeed = foundDevice.GetGazeOutputFrequency();
                    foundDevice.SetGazeOutputFrequency(MAXIMUM_TRACKING_SPEED);
                }
            }

            if (foundDevice.GetGazeOutputFrequency() != MAXIMUM_TRACKING_SPEED)
            {
                ThrottleSkip = Convert.ToInt64((foundDevice.GetGazeOutputFrequency() / MAXIMUM_TRACKING_SPEED) - 1);
                SkipCount = ThrottleSkip;
            }

        }

        public String GetTrackerName()
        {
            return TrackingDevice.DeviceName;
        }

        public String GetTrackerSerialNumber()
        {
            return TrackingDevice.SerialNumber;
        }

        public void StartTracker()
        {
            TrackingDevice.GazeDataReceived += ReceiveRawGaze;
        }

        public void StopTracker()
        {
            TrackingDevice.GazeDataReceived -= ReceiveRawGaze;
        }

        public void EnterCalibration()
        {
            Calibration = new Tobii.Research.ScreenBasedCalibration(TrackingDevice);

            calibrationWindow = new CalibrationWindow();
            calibrationWindow.OnCalibrationPointReached += CalibrationWindow_OnCalibrationPointReached;
            calibrationWindow.OnCalibrationFinished += CalibrationWindow_OnCalibrationFinished;

            Calibration.EnterCalibrationMode();

            calibrationWindow.Show();
        }

        private void CalibrationWindow_OnCalibrationFinished(object sender, EventArgs e)
        {
            TobiiCalibrationResult calibrationResult = new TobiiCalibrationResult(Calibration.ComputeAndApply(), calibrationWindow.ActualWidth, calibrationWindow.ActualHeight);

            if (calibrationResult.IsFailure())
            {
                Console.WriteLine("Calibration failed!");
            }

            SessionManager.GetInstance().SetCalibration(calibrationResult);

            Calibration.LeaveCalibrationMode();

            calibrationWindow.ShowResultsAndClose(calibrationResult.GetLeftEyePoints().ToArray(), calibrationResult.GetRightEyePoints().ToArray());
            SessionManager.GetInstance().SetCalibration(calibrationResult);
        }

        private void CalibrationWindow_OnCalibrationPointReached(object sender, CalibrationPointReachedEventArgs e)
        {
            Tobii.Research.NormalizedPoint2D point = new Tobii.Research.NormalizedPoint2D(
                    (float)(e.CalibrationPoint.X / calibrationWindow.ActualWidth),
                    (float)(e.CalibrationPoint.Y / calibrationWindow.ActualHeight)
                );
            Calibration.CollectData(point);
        }

        public void LeaveCalibration()
        {
            Calibration.LeaveCalibrationMode();
        }

        private void ReceiveRawGaze(object sender, Tobii.Research.GazeDataEventArgs e)
        {
            if (System.Threading.Interlocked.Read(ref SkipCount) == ThrottleSkip)
            {
                GazeHandler.Instance.EnqueueGaze(new TobiiGazeData(e));
                System.Threading.Interlocked.Exchange(ref SkipCount, 0);
            }
            else
            {
                System.Threading.Interlocked.Increment(ref SkipCount);
            }
        }

        public void ShowEyeStatusWindow()
        {
            if (!isEyeStatusOpen)
            {
                eyeStatusWindow = new EyeStatusWindow();

                isEyeStatusOpen = true;

                eyeStatusWindow.Subscribe();

                eyeStatusWindow.Closing += (object sender, CancelEventArgs e) =>
                {
                    eyeStatusWindow.Unsubscribe();
                    isEyeStatusOpen = false;
                };

                eyeStatusWindow.Show();
            }
        }

        ~ TobiiTracker()
        {
            if (previousTrackingSpeed > 0)
            {
                // Restore previous tracking speed
                TrackingDevice.SetGazeOutputFrequency(previousTrackingSpeed);
            }
        }
    }
}
