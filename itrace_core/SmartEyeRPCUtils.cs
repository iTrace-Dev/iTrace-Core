using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    //Classes for serialization of various SmartEye RPC commands and responses
    public class SERPC
    {
        public string jsonrpc = "2.0";
        public string method = "ping";
        public int id = 0;

        //Notification type RPCs do not get a response from the server
        bool isNotification = true;

        public SERPC()
        {

        }

        public SERPC(string methodName)
        {
            this.method = methodName;
        }

        public SERPC(string methodName, bool isNotif)
        {
            this.method = methodName;
            this.isNotification = isNotif;
        }

        //Serialize command to JSON and convert to Netstring
        public String GetNetstring()
        {
            String json = JsonConvert.SerializeObject(this);
            return NetstringUtils.MakeSENetstring(json);
        }
    }

    public class SERPCGetStateResponse : SERPC
    {
        public Dictionary<string, int> responseParams;

        private SERPCGetStateResponse() : base("getState")
        {

        }

        public int? GetState()
        {
            int state;
            bool success = responseParams.TryGetValue("state", out state);

            if (success)
                return state;

            return null;
        }
    }

    public class NetstringUtils
    {
        //Converts raw rpc command into a netstring as defined in the Programmers Guide
        public static String MakeSENetstring(String rpc)
        {
            return rpc.Length.ToString() + ":" + rpc + ",";
        }

        //Converts a netstring to a json string
        //A netstring may look like this 16:{"memes":"dank"},
        //Where 16 is the length of the json before it was formatted to a netstring
        public static String TrimSENetstring(String netstring)
        {
            int length = 0;
            String json = null;

            for (int i = 0; i < netstring.Length; i++)
            {
                //Find end of length prefix
                if (!(netstring[i] >= '0' && netstring[i] <= '9'))
                {
                    //Parse length prefix
                    length = int.Parse(netstring.Substring(0, i));

                    //Trim off length prefix
                    json = netstring.Substring(i + 1);
                    break;
                }
            }

            if (json == null || !json.EndsWith(","))
                throw new Exception("Tried to parse invalid netstring: " + netstring);

            //Trim off trailing comma
            json = json.Substring(0, json.Length - 1);

            //Check length
            if (json.Length != length)
                throw new Exception("Netstring body does not match length prefix: " + netstring);

            return json;
        }
    }
}
