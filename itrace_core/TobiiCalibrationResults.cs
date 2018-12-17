using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml;

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

        void SaveResultsToXML()
        {
            using (var stringWriter = new StringWriter())   //To do: output to a file and not a string
            using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartDocument();

                writer.WriteStartElement("calibration");
                writer.WriteAttributeString("status", "1"); //Should be changed?

                for (int i = 0; i < calibrationResult.CalibrationPoints.Count; ++i)
                {
                    writer.WriteStartElement("point");
                    writer.WriteAttributeString("x", calibrationResult.CalibrationPoints[i].PositionOnDisplayArea.X.ToString());
                    writer.WriteAttributeString("y", calibrationResult.CalibrationPoints[i].PositionOnDisplayArea.Y.ToString());
                    
                    for (int j = 0; j < calibrationResult.CalibrationPoints[i].CalibrationSamples.Count; ++j)
                    {
                        Tobii.Research.CalibrationSample calibrationSample = calibrationResult.CalibrationPoints[i].CalibrationSamples[j];
                        string leftValidity = calibrationSample.LeftEye.Validity == Tobii.Research.CalibrationEyeValidity.InvalidAndNotUsed ? "-1" : "1";
                        string rightValidity = calibrationSample.RightEye.Validity == Tobii.Research.CalibrationEyeValidity.InvalidAndNotUsed ? "-1" : "1";

                        writer.WriteStartElement("sample");

                        writer.WriteAttributeString("left_x", calibrationSample.LeftEye.PositionOnDisplayArea.X.ToString());
                        writer.WriteAttributeString("left_y", calibrationSample.LeftEye.PositionOnDisplayArea.Y.ToString());
                        writer.WriteAttributeString("left_validity", leftValidity);

                        writer.WriteAttributeString("right_x", calibrationSample.RightEye.PositionOnDisplayArea.X.ToString());
                        writer.WriteAttributeString("right_y", calibrationSample.RightEye.PositionOnDisplayArea.Y.ToString());
                        writer.WriteAttributeString("right_validity", rightValidity);

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }
    }
}
