using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace iTrace_Core
{
    class TobiiTracker: ITracker
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

            if(calibrationResult.IsFailure())
            {
                Console.WriteLine("Calibration failed!");
            }

            SessionManager.GetInstance().SetCalibration(calibrationResult);

            Calibration.LeaveCalibrationMode();

            calibrationWindow.ShowResultsAndClose(calibrationResult.GetLeftEyePoints().ToArray(), calibrationResult.GetRightEyePoints().ToArray());
        }

        private void CalibrationWindow_OnCalibrationPointReached(object sender, CalibrationPointReachedEventArgs e)
        {
            Tobii.Research.NormalizedPoint2D point = new Tobii.Research.NormalizedPoint2D(
                    (float) (e.CalibrationPoint.X / calibrationWindow.ActualWidth),
                    (float) (e.CalibrationPoint.Y / calibrationWindow.ActualHeight)
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

            if(isEyeStatusOpen)
            {
                System.Numerics.Vector3 leftEyePosition = new System.Numerics.Vector3(e.LeftEye.GazeOrigin.PositionInUserCoordinates.X,
                                                                                      e.LeftEye.GazeOrigin.PositionInUserCoordinates.Y,
                                                                                      e.LeftEye.GazeOrigin.PositionInUserCoordinates.Z);

                System.Numerics.Vector3 rightEyePosition = new System.Numerics.Vector3(e.LeftEye.GazeOrigin.PositionInUserCoordinates.X,
                                                                                       e.LeftEye.GazeOrigin.PositionInUserCoordinates.Y,
                                                                                       e.LeftEye.GazeOrigin.PositionInUserCoordinates.Z);

                eyeStatusWindow.UpdateEyePosition(leftEyePosition, rightEyePosition);
            }
        }

        public void ShowEyeStatusWindow()
        {
            if (!isEyeStatusOpen)
            {
                eyeStatusWindow = new EyeStatusWindow();

                isEyeStatusOpen = true;

                eyeStatusWindow.Closing += (object sender, CancelEventArgs e) =>
                {
                    isEyeStatusOpen = false;
                };

                eyeStatusWindow.Show();
            }
        }
    }
}
