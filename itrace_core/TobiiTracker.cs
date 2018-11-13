using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace iTrace_Core
{
    class TobiiTracker: ITracker
    {
        private readonly Tobii.Research.IEyeTracker TrackingDevice;
        private Tobii.Research.ScreenBasedCalibration Calibration;
        private CalibrationWindow calibrationWindow;

        public TobiiTracker(Tobii.Research.IEyeTracker foundDevice)
        {
            TrackingDevice = foundDevice;
        }

        public String GetTrackerName()
        {
            return TrackingDevice.DeviceName;
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
            Tobii.Research.CalibrationResult calibrationResult = Calibration.ComputeAndApply();

            if(calibrationResult.Status == Tobii.Research.CalibrationStatus.Failure)
            {
                Console.WriteLine("Calibration failed!");
            }
            
            Calibration.LeaveCalibrationMode();


            List<Point> leftEyePoints = new List<Point>();
            List<Point> rightEyePoints = new List<Point>();

            for (int i = 0; i < calibrationResult.CalibrationPoints.Count; ++i)
            {
                for(int j = 0; j < calibrationResult.CalibrationPoints[i].CalibrationSamples.Count; ++j)
                {
                    leftEyePoints.Add(new Point(
                            calibrationResult.CalibrationPoints[i].CalibrationSamples[j].LeftEye.PositionOnDisplayArea.X * calibrationWindow.ActualWidth,
                            calibrationResult.CalibrationPoints[i].CalibrationSamples[j].LeftEye.PositionOnDisplayArea.Y * calibrationWindow.Height
                        ));

                    rightEyePoints.Add(new Point(
                            calibrationResult.CalibrationPoints[i].CalibrationSamples[j].RightEye.PositionOnDisplayArea.X * calibrationWindow.ActualWidth,
                            calibrationResult.CalibrationPoints[i].CalibrationSamples[j].RightEye.PositionOnDisplayArea.Y * calibrationWindow.Height
                        ));
                }
            }


            calibrationWindow.ShowResultsAndClose(leftEyePoints.ToArray(), rightEyePoints.ToArray());
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

        private static void ReceiveRawGaze(object sender, Tobii.Research.GazeDataEventArgs e)
        {
            /*
            Console.WriteLine("Left Eye: ({0},{1}) - Pupil: {2}({3}) - Z: {4} - {5}", e.LeftEye.GazePoint.PositionOnDisplayArea.X, e.LeftEye.GazePoint.PositionOnDisplayArea.Y,
                e.LeftEye.Pupil.PupilDiameter, e.LeftEye.Pupil.Validity, e.LeftEye.GazePoint.PositionInUserCoordinates.Z, e.LeftEye.GazePoint.Validity);
            Console.WriteLine("Right Eye: ({0},{1}) - Pupil: {2}({3}) - Z: {4} - {5}", e.RightEye.GazePoint.PositionOnDisplayArea.X, e.RightEye.GazePoint.PositionOnDisplayArea.Y,
                e.RightEye.Pupil.PupilDiameter, e.LeftEye.Pupil.Validity, e.RightEye.GazePoint.PositionInUserCoordinates.Z, e.RightEye.GazePoint.Validity);
            */
            GazeHandler.Instance.EnqueueGaze(new TobiiGazeData(e));
        }
    }
}
