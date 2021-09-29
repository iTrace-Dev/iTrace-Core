using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    //A SmartEye world model which can be parsed from or serialized to a string
    class SEWorldModel
    {
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

    }

    class SEWorldPlane
    {
        public string name { get; private set; }
        public Vector3 lowerLeft { get; private set; }
        public Vector3 xAxis { get; private set; }
        public Vector3 yAxis { get; private set; }
        public Vector2 size { get; private set; }
        //public int[] resolution { get; private set; }

        public static string example = "\tname = \"ControllerLeft\"\n\tlowerLeft = 0.086, -0.003, 0.249\n\txAxis =  0.087, 0.011, 0.039\n\tyAxis = 0.042, 0.047, -0.156\n\tsize = 0.096, 0.168";

        //Parse a Plane from the world model, takes the contents of the Plane block inside and excluding curly braces
        public SEWorldPlane(String screenBlock)
        {
            string[] lines = screenBlock.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length != 5)
                throw new Exception("Invalid Plane string: " + screenBlock);

            foreach (string line in lines)
            { 
                string[] terms = line.Split('=');

                if (terms.Length != 2)
                    throw new Exception("Line contains more than one assignment?: " + line);

                string asignee = terms[0].Trim();
                string value = terms[1].Trim();

                switch (asignee)
                {
                    case "name":
                        this.name = value.Replace("\"","");
                        break;

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

                    default:
                        throw new Exception("Invalid field while parsing: " + line);
                }
            }
        }
    }
}
