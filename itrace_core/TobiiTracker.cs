﻿using System;
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

        public TobiiTracker(Tobii.Research.IEyeTracker foundDevice)
        {
            TrackingDevice = foundDevice;
            isEyeStatusOpen = false;
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
            
            Calibration.LeaveCalibrationMode();
            SessionManager.GetInstance().SetCalibration(calibrationResult, this);

            calibrationWindow.ShowResultsAndClose(calibrationResult.GetLeftEyePoints().ToArray(), calibrationResult.GetRightEyePoints().ToArray());

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
            GazeHandler.Instance.EnqueueGaze(new TobiiGazeData(e));
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
    }
}
