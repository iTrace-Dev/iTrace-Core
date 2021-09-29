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

    abstract class SEWorldObject
    {
        public string name { get; protected set; }

        protected SEWorldObject(string block)
        {
            string[] lines = block.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
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
    }
}
