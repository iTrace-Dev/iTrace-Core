using System;
using System.Text;
using System.Threading;
using System.Windows;
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

        public void StartWriting(string filename)
        {
            Writing = true;
            xmlTextWriter = new XmlTextWriter(filename, Encoding.UTF8);

            xmlTextWriter.Formatting = Formatting.Indented;

            xmlTextWriter.WriteStartDocument();
            xmlTextWriter.WriteStartElement("core");

            WriteEnvironment();
        }

        private void WriteEnvironment()
        {
            xmlTextWriter.WriteStartElement("environment");

            xmlTextWriter.WriteStartElement("screen-size");
            xmlTextWriter.WriteAttributeString("width", SystemParameters.PrimaryScreenWidth.ToString());
            xmlTextWriter.WriteAttributeString("height", SystemParameters.PrimaryScreenHeight.ToString());
            xmlTextWriter.WriteEndElement();

            xmlTextWriter.WriteStartElement("eye-tracker");
            xmlTextWriter.WriteAttributeString("type", "mouse"); //TODO: Change to current tracker
            xmlTextWriter.WriteEndElement();

            xmlTextWriter.WriteStartElement("date");
            xmlTextWriter.WriteValue(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            xmlTextWriter.WriteEndElement();

            xmlTextWriter.WriteStartElement("time");
            xmlTextWriter.WriteValue(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            xmlTextWriter.WriteEndElement();

            xmlTextWriter.WriteStartElement("session");
            xmlTextWriter.WriteAttributeString("id", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
            xmlTextWriter.WriteEndElement();

            //TODO: calibration id, study, researcher, participant, etc...

            xmlTextWriter.WriteEndElement();
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

            xmlTextWriter.WriteEndElement();
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
