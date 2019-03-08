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
            xmlTextWriter.WriteAttributeString("session_data_time", SessionManager.GetInstance().CurrentSessionTimeStamp);
            xmlTextWriter.WriteAttributeString("task_name", SessionManager.GetInstance().TaskName);
            xmlTextWriter.WriteAttributeString("researcher", SessionManager.GetInstance().ResearcherName);
            xmlTextWriter.WriteAttributeString("participant_id", SessionManager.GetInstance().ParticipantID);
        }

        private void WriteEnvironment()
        {
            xmlTextWriter.WriteStartElement("environment");

            xmlTextWriter.WriteAttributeString("screen_width", SessionManager.GetInstance().ScreenWidth.ToString());
            xmlTextWriter.WriteAttributeString("session_height", SessionManager.GetInstance().ScreenHeight.ToString());
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

            xmlTextWriter.WriteAttributeString("user_left_x", gazeData.UserLeftX.ToString());
            xmlTextWriter.WriteAttributeString("user_left_y", gazeData.UserLeftY.ToString());
            xmlTextWriter.WriteAttributeString("user_left_z", gazeData.UserLeftZ.ToString());

            xmlTextWriter.WriteAttributeString("user_right_x", gazeData.UserRightX.ToString());
            xmlTextWriter.WriteAttributeString("user_right_y", gazeData.UserRightY.ToString());
            xmlTextWriter.WriteAttributeString("user_right_z", gazeData.UserRightZ.ToString());

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
