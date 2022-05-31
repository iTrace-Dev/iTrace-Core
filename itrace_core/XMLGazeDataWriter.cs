/********************************************************************************************************************************************************
* @file XMLGazeDataWriter.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

using System.Text;
using System.Threading;
using System.Xml;

namespace iTrace_Core
{
    class XMLGazeDataWriter
    {
        public bool Writing { get; private set; }
        XmlTextWriter xmlTextWriter;
        Mutex mutex = new Mutex();  // Prevents gaze data from being written to file when xml file has been closed. 

        public XMLGazeDataWriter()
        {
            GazeHandler.Instance.OnGazeDataReceived += ReceiveGazeData;
        }

        public void StartWriting(string rootDirectory)
        {
            xmlTextWriter = new XmlTextWriter(rootDirectory + "/itrace_core-" + SessionManager.GetInstance().CurrentSessionTimeStamp + ".xml", Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlTextWriter.WriteStartDocument();

            WriteSessionInformation();
            WriteEnvironment();
            WriteCalibration();
            WriteOpeningGazeTag();

            Writing = true;
        }

        private void WriteSessionInformation()
        {
            xmlTextWriter.WriteStartElement("itrace_core");

            xmlTextWriter.WriteAttributeString("session_id", SessionManager.GetInstance().CurrentSessionID);
            xmlTextWriter.WriteAttributeString("session_date_time", SessionManager.GetInstance().CurrentSessionTimeStamp);
            xmlTextWriter.WriteAttributeString("task_name", SessionManager.GetInstance().TaskName);
            xmlTextWriter.WriteAttributeString("researcher", SessionManager.GetInstance().ResearcherName);
            xmlTextWriter.WriteAttributeString("participant_id", SessionManager.GetInstance().ParticipantID);
        }

        private void WriteEnvironment()
        {
            xmlTextWriter.WriteStartElement("environment");

            xmlTextWriter.WriteAttributeString("screen_width", SessionManager.GetInstance().ScreenWidth.ToString());
            xmlTextWriter.WriteAttributeString("screen_height", SessionManager.GetInstance().ScreenHeight.ToString());
            xmlTextWriter.WriteAttributeString("tracker_type", SessionManager.GetInstance().TrackerType);
            xmlTextWriter.WriteAttributeString("tracker_serial_number", SessionManager.GetInstance().TrackerSerialNumber);
            xmlTextWriter.WriteAttributeString("screen_recording_start", SessionManager.GetInstance().ScreenRecordingStart);

            xmlTextWriter.WriteEndElement();
        }

        private void WriteCalibration()
        {
            SessionManager.GetInstance().CurrentCalibration.WriteToXMLWriter(xmlTextWriter);
        }

        private void WriteOpeningGazeTag()
        {
            xmlTextWriter.WriteStartElement("gazes");
        }

        private void WriteGaze(GazeData gazeData)
        {
            xmlTextWriter.WriteStartElement("response");

            xmlTextWriter.WriteAttributeString("event_id", gazeData.EventTime.ToString());
            xmlTextWriter.WriteAttributeString("core_time", gazeData.SystemTime.ToString());
            xmlTextWriter.WriteAttributeString("tracker_time", gazeData.TrackerTime.ToString());

            xmlTextWriter.WriteAttributeString("x", gazeData.X.HasValue ? gazeData.X.ToString() : "NaN");
            xmlTextWriter.WriteAttributeString("y", gazeData.Y.HasValue ? gazeData.Y.ToString() : "NaN");

            xmlTextWriter.WriteAttributeString("left_x", gazeData.LeftX.ToString());
            xmlTextWriter.WriteAttributeString("left_y", gazeData.LeftY.ToString());

            xmlTextWriter.WriteAttributeString("left_pupil_diameter", gazeData.LeftPupil.ToString());
            xmlTextWriter.WriteAttributeString("left_validation", gazeData.LeftValidation.ToString());

            xmlTextWriter.WriteAttributeString("right_x", gazeData.RightX.ToString());
            xmlTextWriter.WriteAttributeString("right_y", gazeData.RightY.ToString());

            xmlTextWriter.WriteAttributeString("right_pupil_diameter", gazeData.RightPupil.ToString());
            xmlTextWriter.WriteAttributeString("right_validation", gazeData.RightValidation.ToString());

            xmlTextWriter.WriteAttributeString("user_left_x", gazeData.UserLeftX.ToString());
            xmlTextWriter.WriteAttributeString("user_left_y", gazeData.UserLeftY.ToString());
            xmlTextWriter.WriteAttributeString("user_left_z", gazeData.UserLeftZ.ToString());

            xmlTextWriter.WriteAttributeString("user_right_x", gazeData.UserRightX.ToString());
            xmlTextWriter.WriteAttributeString("user_right_y", gazeData.UserRightY.ToString());
            xmlTextWriter.WriteAttributeString("user_right_z", gazeData.UserRightZ.ToString());

            //TODO: kinda bad, this method should actually call a method in GazeData which can be overidden for extra attributes
            if (gazeData is SmartEyeGazeData)
            {
                SmartEyeGazeData segd = gazeData as SmartEyeGazeData;

                xmlTextWriter.WriteAttributeString("intersection_name", segd.intersectionName);
            }

            xmlTextWriter.WriteEndElement();
        }

        public void StopWriting()
        {
            mutex.WaitOne();

            xmlTextWriter.WriteEndDocument();

            xmlTextWriter.Flush();
            xmlTextWriter.Dispose();

            Writing = false;

            mutex.ReleaseMutex();
        }

        private void ReceiveGazeData(object sender, GazeDataReceivedEventArgs e)
        {
            mutex.WaitOne();

            if (Writing)
            {
                WriteGaze(e.ReceivedGazeData);
            }

            mutex.ReleaseMutex();
        }
    }
}
