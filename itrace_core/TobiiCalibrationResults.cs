using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;

namespace iTrace_Core
{
    class TobiiCalibrationResults
    {
        private Tobii.Research.CalibrationResult calibrationResult;
        private double calibrationScreenWidth;
        private double calibrationScreenHeight;

        public TobiiCalibrationResults(Tobii.Research.CalibrationResult calibrationResult, double calibrationScreenWidth, double calibrationScreenHeight)
        {
            this.calibrationResult = calibrationResult;
            this.calibrationScreenWidth = calibrationScreenWidth;
            this.calibrationScreenHeight = calibrationScreenHeight;
        }

        public List<Point> GetLeftEyePoints()
        {
            List<Point> leftEyePoints = new List<Point>();

            for (int i = 0; i < calibrationResult.CalibrationPoints.Count; ++i)
            {
                for (int j = 0; j < calibrationResult.CalibrationPoints[i].CalibrationSamples.Count; ++j)
                {
                    leftEyePoints.Add(new Point(
                            calibrationResult.CalibrationPoints[i].CalibrationSamples[j].LeftEye.PositionOnDisplayArea.X * calibrationScreenWidth,
                            calibrationResult.CalibrationPoints[i].CalibrationSamples[j].LeftEye.PositionOnDisplayArea.Y * calibrationScreenHeight
                        ));
                }
            }

            return leftEyePoints;
        }

        public List<Point> GetRightEyePoints()
        {
            List<Point> rightEyePoints = new List<Point>();

            for (int i = 0; i < calibrationResult.CalibrationPoints.Count; ++i)
            {
                for (int j = 0; j < calibrationResult.CalibrationPoints[i].CalibrationSamples.Count; ++j)
                {
                    rightEyePoints.Add(new Point(
                            calibrationResult.CalibrationPoints[i].CalibrationSamples[j].RightEye.PositionOnDisplayArea.X * calibrationScreenWidth,
                            calibrationResult.CalibrationPoints[i].CalibrationSamples[j].RightEye.PositionOnDisplayArea.Y * calibrationScreenHeight
                        ));
                }
            }

            return rightEyePoints;
        }

        public bool IsFailure()
        {
            return calibrationResult.Status == Tobii.Research.CalibrationStatus.Failure;
        }

        private String createPointSample(float leftX, float leftY, bool leftValidity, float rightX, float rightY, bool rightValidity)
        {
            String leftValidityString = leftValidity ? "1" : "-1";
            String rightValidityString = rightValidity ? "1" : "-1";

            return "<sample left_x=\"" + leftX.ToString() + "\" left_y=\"" + leftY.ToString() + "\" left_validity=\"" + leftValidityString + "\" right_x=\"" +
                rightX.ToString() + "\" right_y=\"" + rightY.ToString() + "\" right_validity=\"" + rightValidityString + "\"/>";
        }

        void SaveResultsToXML()
        {
            StringBuilder calibrationResultsXML = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");

            calibrationResultsXML.AppendLine("<calibration status=\"1\">"); // Status should be changed?

            for(int i = 0; i < calibrationResult.CalibrationPoints.Count; ++i)
            {
                string pointX = calibrationResult.CalibrationPoints[i].PositionOnDisplayArea.X.ToString();
                string pointY = calibrationResult.CalibrationPoints[i].PositionOnDisplayArea.Y.ToString();
                calibrationResultsXML.AppendLine("<point x=\"" + pointX + "\" y=\"" + pointY + "\">");

                for(int j = 0; j < calibrationResult.CalibrationPoints[i].CalibrationSamples.Count; ++j)
                {
                    Tobii.Research.CalibrationSample sample = calibrationResult.CalibrationPoints[i].CalibrationSamples[j];
                    calibrationResultsXML.AppendLine(createPointSample(
                        sample.LeftEye.PositionOnDisplayArea.X,
                        sample.LeftEye.PositionOnDisplayArea.Y,
                        sample.LeftEye.Validity != Tobii.Research.CalibrationEyeValidity.InvalidAndNotUsed,
                        sample.RightEye.PositionOnDisplayArea.X,
                        sample.RightEye.PositionOnDisplayArea.Y,
                        sample.RightEye.Validity != Tobii.Research.CalibrationEyeValidity.InvalidAndNotUsed
                        ));
                }
            }

            calibrationResultsXML.AppendLine("</calibration>");

            string result = calibrationResultsXML.ToString();

            //To do: output result to a file
        }
    }
}
