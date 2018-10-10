using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    class TobiiTracker: ITracker
    {
        private readonly Tobii.Research.IEyeTracker TrackingDevice;
        private Tobii.Research.ScreenBasedCalibration Calibration;

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
        }

        public void LeaveCalibration()
        {
            Calibration.LeaveCalibrationMode();
        }

        private static void ReceiveRawGaze(object sender, Tobii.Research.GazeDataEventArgs e)
        {
            Console.WriteLine("Left Eye: ({0},{1}) - Z: {2} - {3}", e.LeftEye.GazePoint.PositionOnDisplayArea.X, e.LeftEye.GazePoint.PositionOnDisplayArea.Y,
                e.LeftEye.GazePoint.PositionInUserCoordinates.Z, e.LeftEye.GazePoint.Validity);
            Console.WriteLine("Right Eye: ({0},{1}) - Z: {2} - {3}", e.RightEye.GazePoint.PositionOnDisplayArea.X, e.RightEye.GazePoint.PositionOnDisplayArea.Y,
                e.RightEye.GazePoint.PositionInUserCoordinates.Z, e.RightEye.GazePoint.Validity);
        }
    }
}
