using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Xml;

namespace iTrace_Core
{
    class XMLGazeDataWriter
    {
        public bool Writing { get; private set; }
        XmlTextWriter xmlTextWriter;

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
            //TODO: GazeData needs to be expanded to contain information for all the attributes

            xmlTextWriter.WriteStartElement("response");

            xmlTextWriter.WriteAttributeString("x", gazeData.X.ToString());
            xmlTextWriter.WriteAttributeString("y", gazeData.Y.ToString());
            
            xmlTextWriter.WriteAttributeString("left_x", gazeData.X.ToString());
            xmlTextWriter.WriteAttributeString("left_y", gazeData.Y.ToString());

            xmlTextWriter.WriteAttributeString("left_pupil_diameter", "0");
            xmlTextWriter.WriteAttributeString("left_validation", "1");

            xmlTextWriter.WriteAttributeString("right_x", gazeData.X.ToString());
            xmlTextWriter.WriteAttributeString("right_y", gazeData.Y.ToString());

            xmlTextWriter.WriteAttributeString("right_pupil_diameter", "0");
            xmlTextWriter.WriteAttributeString("right_validation", "1");

            xmlTextWriter.WriteAttributeString("tracker_time", "0");
            xmlTextWriter.WriteAttributeString("system_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
            xmlTextWriter.WriteAttributeString("event_time", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());    // No nanoseconds in c#

            xmlTextWriter.WriteEndElement();
        }

        public void StopWriting()
        {
            xmlTextWriter.WriteEndElement();
            xmlTextWriter.WriteEndDocument();

            xmlTextWriter.Flush();
            xmlTextWriter.Dispose();

            Writing = false;
        }

        private void ReceiveGazeData(object sender, GazeDataReceivedEventArgs e)
        {
            if(Writing)
            {
                WriteGaze(e.ReceivedGazeData);
            }
        }
    }
}
