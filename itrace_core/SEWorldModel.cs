using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace iTrace_Core
{
    //A SmartEye world model which can be parsed from or serialized to a string
    class SEWorldModel
    {
        private List<SEWorldObject> objects;

        public SEWorldModel(string worldModelString)
        {
            objects = new List<SEWorldObject>();

            //If a // is encountered, skip to the next new line
            //If a keyword is encountered, switch on that keyword and pass the contents of the next curly brackets to the respective class constructor

            int i = 0; //Start of imaginary cursor for parsing words
            int startOfLine = 0;

            string worldModelCode = "";

            //Strip comments only
            while (i < worldModelString.Length - 1)
            {
                //Consume comment line
                if (worldModelString.Substring(i, 2).Equals("//"))
                {
                    int k = i; //Move start cursor to end of //       
                    while (!worldModelString[k++].Equals('\n')) ;
                    string code = worldModelString.Substring(startOfLine, i - startOfLine);
                    string comment = worldModelString.Substring(i + 2, k - i - 2);

                    worldModelCode += code;

                    if (!String.IsNullOrWhiteSpace(code) && !String.IsNullOrWhiteSpace(comment))
                        worldModelCode += "\n";

                    startOfLine = i + 1;
                    i = k - 1; //Move start cursor to after new line
                } else if (worldModelString[i].Equals('\n'))
                {
                    worldModelCode += worldModelString.Substring(startOfLine, i - startOfLine);
                    startOfLine = i + 1;
                }

                //This is not redundant i promise
                if (worldModelString[i].Equals('\n'))
                    startOfLine = i + 1;

                i++;
            }

            i = 0;
            int j = 0;

            //TODO: huge number of messy edge cases. Need better string normalization
            while (j < worldModelCode.Length)
            {
                //Consume word
                if (Char.IsWhiteSpace(worldModelCode[j]))
                {
                    string word = worldModelCode.Substring(i, j - i).Trim();

                    switch (word)
                    {
                        case "Screen":
                            objects.Add(new SEWorldScreen(PeelBraces(worldModelCode, i)));
                            break;
                        case "Plane":
                            objects.Add(new SEWorldPlane(PeelBraces(worldModelCode, i)));
                            break;
                        case "CalibrationPoint3D":
                            objects.Add(new SEWorldCalibrationPoint(PeelBraces(worldModelCode, i)));
                            break;
                    }

                    i = j + 1; //Move start cursor to after word
                }

                j++;
            }
        }

        //Removes one layer of braces from a code block
        public static string PeelBraces(string bracedBlock, int startIndex)
        {
            int i = startIndex;
            int j = i;

            while (!bracedBlock[i++].Equals('{')) ;
            j = i;

            //Number of brace layers (this may not be necessary, check with programming guide to see if nested braces even exist in WorldModel format)
            int braceLevel = 1;

            while (j < bracedBlock.Length)
            {
                if (bracedBlock[j].Equals('{'))
                    braceLevel++;
                else if (bracedBlock[j].Equals('}'))
                    braceLevel--;

                if (braceLevel == 0)
                    break;

                j++;
            }

            return bracedBlock.Substring(i, j - i);
        }

        //Doesn't go here
        public static Vector3 ParseVector3(string vectorString)
        {
            string[] terms = vectorString.Split(',');

            if (terms.Length != 3)
                throw new Exception("Invalid vector string: " + vectorString);

            return new Vector3(float.Parse(terms[0]), float.Parse(terms[1]), float.Parse(terms[2]));
        }

        public static Vector2 ParseVector2(string vectorString)
        {
            string[] terms = vectorString.Split(',');

            if (terms.Length != 2)
                throw new Exception("Invalid vector string: " + vectorString);

            return new Vector2(float.Parse(terms[0]), float.Parse(terms[1]));
        }

        public void WriteToXMLWriter(XmlTextWriter writer)
        {
            writer.WriteStartElement("worldModel");

            foreach (SEWorldObject worldObject in objects)
                worldObject.WriteToXMLWriter(writer);

            writer.WriteEndElement();
        }
    }

    abstract class SEWorldObject
    {
        public string name { get; protected set; }

        protected SEWorldObject(string block)
        {
            string[] lines = block.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                if (String.IsNullOrWhiteSpace(line))
                    continue;

                string[] terms = line.Split('=');

                if (terms.Length != 2)
                    throw new Exception("Line contains more than one assignment?: " + line);

                string assignee = terms[0].Trim();
                string value = terms[1].Trim();

                ParseParameter(assignee, value);
            }

            if (name == null)
                throw new Exception("SEWorld object Not fully initialized!");
        }

        protected virtual void ParseParameter(string assignee, string value)
        {
            if (assignee == "name")
                this.name = value.Replace("\"", "");
        }

        public abstract void WriteToXMLWriter(XmlTextWriter writer);
    }

    class SEWorldPlane : SEWorldObject
    {
        public Vector3 lowerLeft { get; private set; }
        public Vector3 xAxis { get; private set; }
        public Vector3 yAxis { get; private set; }
        public Vector2 size { get; private set; }

        public static string example = "\tname = \"ControllerLeft\"\n\tlowerLeft = 0.086, -0.003, 0.249\n\txAxis =  0.087, 0.011, 0.039\n\tyAxis = 0.042, 0.047, -0.156\n\tsize = 0.096, 0.168";

        //Parse a Plane from the world model, takes the contents of the Plane block inside and excluding curly braces
        public SEWorldPlane(String planeBlock) : base(planeBlock)
        {
            if (!(lowerLeft != null && xAxis != null && yAxis != null && size != null))
                throw new Exception("SEWorld object Not fully initialized!");
        }

        protected override void ParseParameter(string assignee, string value)
        {
            base.ParseParameter(assignee, value);

            switch (assignee)
            {
                case "lowerLeft":
                    this.lowerLeft = SEWorldModel.ParseVector3(value);
                    break;

                case "xAxis":
                    this.xAxis = SEWorldModel.ParseVector3(value);
                    break;

                case "yAxis":
                    this.yAxis = SEWorldModel.ParseVector3(value);
                    break;

                case "size":
                    this.size = SEWorldModel.ParseVector2(value);
                    break;
            }
        }

        protected virtual void WriteXMLAttributes(XmlTextWriter writer)
        {
            writer.WriteAttributeString("name", name);
            writer.WriteAttributeString("lowerLeft", lowerLeft.ToString());
            writer.WriteAttributeString("xAxis", xAxis.ToString());
            writer.WriteAttributeString("yAxis", yAxis.ToString());
            writer.WriteAttributeString("size", size.ToString());
        }

        public override void WriteToXMLWriter(XmlTextWriter writer)
        {
            writer.WriteStartElement("plane");
            WriteXMLAttributes(writer);
            writer.WriteEndElement();
        }
    }

    class SEWorldScreen : SEWorldPlane
    {
        public int[] resolution { get; private set; }

        public static string example = "name = \"ScreenRight\"\n\tlowerLeft = 0.023, 0.058, -0.175\n\txAxis = 0.573, 0.005, 0.172\n\tyAxis = -0.002, 0.329, -0.006\n\tsize = 0.598, 0.329\n\tresolution = 1920, 1080";

        protected override void ParseParameter(string assignee, string value)
        {
            base.ParseParameter(assignee, value);

            if (assignee == "resolution")
            {
                string[] res = value.Split(',');

                this.resolution = new int[2];
                this.resolution[0] = int.Parse(res[0]);
                this.resolution[1] = int.Parse(res[1]);
            }
        }

        public SEWorldScreen(string screenBlock) : base(screenBlock)
        {
            if (resolution == null)
                throw new Exception("SEWorld object Not fully initialized!");
        }

        protected override void WriteXMLAttributes(XmlTextWriter writer)
        {
            base.WriteXMLAttributes(writer);
            writer.WriteAttributeString("resolution", "(" + resolution[0].ToString() + "," + resolution[1].ToString() + ")" );
        }

        public override void WriteToXMLWriter(XmlTextWriter writer)
        {
            writer.WriteStartElement("screen");
            WriteXMLAttributes(writer);
            writer.WriteEndElement();
        }
    }

    class SEWorldCalibrationPoint : SEWorldObject
    {
        public Vector3 center { get; private set; }

        protected override void ParseParameter(string assignee, string value)
        {
            base.ParseParameter(assignee, value);

            if (assignee == "center")
                this.center = SEWorldModel.ParseVector3(value);
        }

        public SEWorldCalibrationPoint(string block) : base(block)
        {
            if (center == null)
                throw new Exception("SEWorld object Not fully initialized!");
        }

        public override void WriteToXMLWriter(XmlTextWriter writer)
        {
            writer.WriteStartElement("calibrationPoint3D");
            writer.WriteAttributeString("center", center.ToString());
            writer.WriteEndElement();
        }
    }
}
