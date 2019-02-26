using System.Collections.Generic;
using System.Windows;
using System.Xml;

namespace iTrace_Core
{
    abstract class CalibrationResult
    {
        public abstract void WriteToXMLWriter(XmlTextWriter xmlTextWriter, string timestamp);
    }

    class EmptyCalibrationResult : CalibrationResult
    {
        public override void WriteToXMLWriter(XmlTextWriter xmlTextWriter, string timestamp) { }
    }

    class TobiiCalibrationResult : CalibrationResult
    {
        private Tobii.Research.CalibrationResult calibrationResult;
        private double calibrationScreenWidth;
        private double calibrationScreenHeight;

        public TobiiCalibrationResult(Tobii.Research.CalibrationResult calibrationResult, double calibrationScreenWidth, double calibrationScreenHeight)
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

        public override void WriteToXMLWriter(XmlTextWriter xmlTextWriter, string timestamp)
        {
            xmlTextWriter.WriteStartElement("calibration");
            xmlTextWriter.WriteAttributeString("timestamp", "[timestamp_milli]");

            for (int i = 0; i < calibrationResult.CalibrationPoints.Count; ++i)
            {
                xmlTextWriter.WriteStartElement("calibration_point");
                xmlTextWriter.WriteAttributeString("x", calibrationResult.CalibrationPoints[i].PositionOnDisplayArea.X.ToString());
                xmlTextWriter.WriteAttributeString("y", calibrationResult.CalibrationPoints[i].PositionOnDisplayArea.Y.ToString());

                for (int j = 0; j < calibrationResult.CalibrationPoints[i].CalibrationSamples.Count; ++j)
                {
                    Tobii.Research.CalibrationSample calibrationSample = calibrationResult.CalibrationPoints[i].CalibrationSamples[j];
                    string leftValidity = calibrationSample.LeftEye.Validity == Tobii.Research.CalibrationEyeValidity.InvalidAndNotUsed ? "-1" : "1";
                    string rightValidity = calibrationSample.RightEye.Validity == Tobii.Research.CalibrationEyeValidity.InvalidAndNotUsed ? "-1" : "1";

                    xmlTextWriter.WriteStartElement("sample");

                    xmlTextWriter.WriteAttributeString("left_x", calibrationSample.LeftEye.PositionOnDisplayArea.X.ToString());
                    xmlTextWriter.WriteAttributeString("left_y", calibrationSample.LeftEye.PositionOnDisplayArea.Y.ToString());
                    xmlTextWriter.WriteAttributeString("left_validity", leftValidity);

                    xmlTextWriter.WriteAttributeString("right_x", calibrationSample.RightEye.PositionOnDisplayArea.X.ToString());
                    xmlTextWriter.WriteAttributeString("right_y", calibrationSample.RightEye.PositionOnDisplayArea.Y.ToString());
                    xmlTextWriter.WriteAttributeString("right_validity", rightValidity);

                    xmlTextWriter.WriteEndElement();
                }

                xmlTextWriter.WriteEndElement();
            }

            xmlTextWriter.WriteEndElement();
        }
    }

    class GazePointCalibrationResult : CalibrationResult
    {

        XmlDocument XmlDoc = null;
        int CalibrationPointCount = 0;

        public GazePointCalibrationResult(string xmlCalibrationData, int numberOfPoints)
        {
            CalibrationPointCount = numberOfPoints;
            XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(xmlCalibrationData);
        }

        public override void WriteToXMLWriter(XmlTextWriter xmlTextWriter, string timestamp)
        {
            XmlNode recNode = XmlDoc.FirstChild;

            xmlTextWriter.WriteStartElement("calibration");
            xmlTextWriter.WriteAttributeString("timestamp", "[timestamp_milli]");

            for (int i = 1; i <= CalibrationPointCount; ++i)
            {
                xmlTextWriter.WriteStartElement("calibration_point");
                xmlTextWriter.WriteAttributeString("x", recNode.Attributes["CALX" + i].Value);
                xmlTextWriter.WriteAttributeString("y", recNode.Attributes["CALY" + i].Value);

                xmlTextWriter.WriteStartElement("sample");

                xmlTextWriter.WriteAttributeString("left_x", recNode.Attributes["LX" + i].Value);
                xmlTextWriter.WriteAttributeString("left_y", recNode.Attributes["LY" + i].Value);
                xmlTextWriter.WriteAttributeString("left_validity", recNode.Attributes["LV" + i].Value);

                xmlTextWriter.WriteAttributeString("right_x", recNode.Attributes["RX" + i].Value);
                xmlTextWriter.WriteAttributeString("right_y", recNode.Attributes["RY" + i].Value);
                xmlTextWriter.WriteAttributeString("right_validity", recNode.Attributes["RV" + i].Value);

                // Close Sample
                xmlTextWriter.WriteEndElement();

                // Close Calibration Point
                xmlTextWriter.WriteEndElement();
            }

            xmlTextWriter.WriteEndElement();
        }
    }
}
