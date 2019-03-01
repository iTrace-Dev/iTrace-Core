using System;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;
using System.Collections.Generic;

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

        public void StartWriting(string filename)
        {
            xmlTextWriter = new XmlTextWriter(filename, Encoding.UTF8);
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
            xmlTextWriter.WriteAttributeString("session_data_time", "[timestamp_milli]");                       // Todo: Complete this attribute
            xmlTextWriter.WriteAttributeString("session_name", SessionManager.GetInstance().StudyName);
            xmlTextWriter.WriteAttributeString("researcher", SessionManager.GetInstance().ResearcherName);
            xmlTextWriter.WriteAttributeString("participant_id", SessionManager.GetInstance().ParticipantID);
        }
        
        private void WriteEnvironment()
        {
            xmlTextWriter.WriteStartElement("environment");
            
            xmlTextWriter.WriteAttributeString("screen_width", SessionManager.GetInstance().ScreenWidth.ToString());
            xmlTextWriter.WriteAttributeString("session_height", SessionManager.GetInstance().ScreenHeight.ToString());
            xmlTextWriter.WriteAttributeString("tracker_type", "mouse");             // Todo: Get tracker type

            xmlTextWriter.WriteEndElement();
        }

        private void WriteCalibration()
        {
            SessionManager.GetInstance().LastCalibration.WriteToXMLWriter(xmlTextWriter);
        }

        private void WriteOpeningGazeTag()
        {
            xmlTextWriter.WriteStartElement("gazes");
        }

        private void WriteGaze(GazeData gazeData)
        {
            xmlTextWriter.WriteStartElement("response");

            xmlTextWriter.WriteAttributeString("x", gazeData.X.ToString());
            xmlTextWriter.WriteAttributeString("y", gazeData.Y.ToString());

            xmlTextWriter.WriteAttributeString("left_x", gazeData.LeftX.ToString());
            xmlTextWriter.WriteAttributeString("left_y", gazeData.LeftY.ToString());

            xmlTextWriter.WriteAttributeString("left_pupil_diameter", gazeData.LeftPupil.ToString());
            xmlTextWriter.WriteAttributeString("left_validation", gazeData.LeftValidation.ToString());

            xmlTextWriter.WriteAttributeString("right_x", gazeData.RightX.ToString());
            xmlTextWriter.WriteAttributeString("right_y", gazeData.RightY.ToString());

            xmlTextWriter.WriteAttributeString("right_pupil_diameter", gazeData.RightPupil.ToString());
            xmlTextWriter.WriteAttributeString("right_validation", gazeData.RightValidation.ToString());

            xmlTextWriter.WriteAttributeString("tracker_time", gazeData.TrackerTime.ToString());
            xmlTextWriter.WriteAttributeString("system_time", gazeData.SystemTime.ToString());
            xmlTextWriter.WriteAttributeString("event_time", gazeData.EventTime.ToString());

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
