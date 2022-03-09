/********************************************************************************************************************************************************
* @file CalibrationResult.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Xml;

namespace iTrace_Core
{
    abstract class CalibrationResult
    {
        public abstract void WriteToXMLWriter(XmlTextWriter xmlTextWriter);
    }

    class EmptyCalibrationResult : CalibrationResult
    {
        public override void WriteToXMLWriter(XmlTextWriter xmlTextWriter) { }
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

            SessionManager.GetInstance().GenerateCalibrationTimeStamp();
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

        public override void WriteToXMLWriter(XmlTextWriter xmlTextWriter)
        {
            xmlTextWriter.WriteStartElement("calibration");
            xmlTextWriter.WriteAttributeString("timestamp", SessionManager.GetInstance().CurrentCalibrationTimeStamp);

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

            SessionManager.GetInstance().GenerateCalibrationTimeStamp();
        }

        public override void WriteToXMLWriter(XmlTextWriter xmlTextWriter)
        {
            XmlNode recNode = XmlDoc.FirstChild;

            xmlTextWriter.WriteStartElement("calibration");
            xmlTextWriter.WriteAttributeString("timestamp", SessionManager.GetInstance().CurrentCalibrationTimeStamp);

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

    class SmartEyeCalibrationResult : CalibrationResult
    {
        //These are the calibration vectors produced by the SE gaze calibration dialogue
        private List<SETarget> calibrationTargets;
        private SEWorldModel worldModel;
        public ScreenMapping screenMapping { get; private set; }

        public SmartEyeCalibrationResult(SEWorldModel worldModel, List<SETarget> calibrationTargets)
        {
            //TODO: world model sanity checks

            this.worldModel = worldModel;
            this.calibrationTargets = calibrationTargets;
            this.screenMapping = new ScreenMapping(worldModel.GetScreens(), Screen.AllScreens);

            SessionManager.GetInstance().GenerateCalibrationTimeStamp();
        }

        public override void WriteToXMLWriter(XmlTextWriter xmlTextWriter)
        {
            worldModel.WriteToXMLWriter(xmlTextWriter);

            xmlTextWriter.WriteStartElement("calibration");
            xmlTextWriter.WriteAttributeString("timestamp", SessionManager.GetInstance().CurrentCalibrationTimeStamp);

            foreach (SETarget target in calibrationTargets)
            {
                xmlTextWriter.WriteStartElement("calibration_point");
                xmlTextWriter.WriteAttributeString("targetId", target.targetId.ToString());

                //Write X and Y of calibration point as percent of screen size. This may require extracting from the world model string!
                //xmlTextWriter.WriteAttributeString("x", "0.5");
                //xmlTextWriter.WriteAttributeString("y", "0.5");

                int max = target.errorsxl.Length >= target.errorsxr.Length ? target.errorsxl.Length : target.errorsxr.Length;

                for (int i = 0; i < max; i++)
                {
                    xmlTextWriter.WriteStartElement("sample");

                    if (i < target.errorsxl.Length)
                    {
                        xmlTextWriter.WriteAttributeString("left_x", target.errorsxl[i].ToString());
                        xmlTextWriter.WriteAttributeString("left_y", target.errorsyl[i].ToString());
                        xmlTextWriter.WriteAttributeString("left_validity", "1");
                    } else
                    {
                        xmlTextWriter.WriteAttributeString("left_x", "0.0");
                        xmlTextWriter.WriteAttributeString("left_y", "0.0");
                        xmlTextWriter.WriteAttributeString("left_validity", "0");
                    }

                    if (i < target.errorsxr.Length)
                    {
                        xmlTextWriter.WriteAttributeString("right_x", target.errorsxr[i].ToString());
                        xmlTextWriter.WriteAttributeString("right_y", target.errorsyr[i].ToString());
                        xmlTextWriter.WriteAttributeString("right_validity", "1");
                    }
                    else
                    {
                        xmlTextWriter.WriteAttributeString("right_x", "0.0");
                        xmlTextWriter.WriteAttributeString("right_y", "0.0");
                        xmlTextWriter.WriteAttributeString("right_validity", "0");
                    }

                    xmlTextWriter.WriteEndElement();
                }
               
                xmlTextWriter.WriteEndElement();
            }

            xmlTextWriter.WriteEndElement();
        }
    }
}
