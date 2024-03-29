﻿/********************************************************************************************************************************************************
* @file SmartEyeRPCUtils.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

    public class SERPCOpenTcp
    {
        public string ip;

        public SERPCOpenTcp(IPAddress remoteAddress)
        {

        }
    }

    public class SERPCGetTargetStats : SERPC
    {
        [JsonProperty("params")]
        public int[] rpcParams;

        public SERPCGetTargetStats(int targetId) : base("retrieveTargetStatistics", false)
        {
            rpcParams = new int[1];
            rpcParams[0] = targetId;
        }
    }

    //Put this somewhere else
    //SmartEye target calibration result, deserialized from a call to retrieveTargetStatistics
    public class SETarget
    {
        public int targetId;

        //Stats
        public double[] stdDev;
        public double[] accuracy;

        //Error vectors
        public double[] errorsxl;
        public double[] errorsyl;
        public double[] errorsxr;
        public double[] errorsyr;

        //A target should only be considered if it has at least one error vector in either eye
        public Boolean TargetValid()
        {
            return errorsxl.Length > 0 || errorsxr.Length > 0;
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

            //Netstring must end with comma
            if (!(netstring[netstring.Length - 1] == ','))
                throw new ArgumentException("Netstring does not end with a comma");

            for (int i = 0; i < netstring.Length; i++)
            {
                //Find end of length prefix
                if (!(netstring[i] >= '0' && netstring[i] <= '9'))
                {
                    if (i == 0)
                        throw new ArgumentException("Netstring does not start with a length field");

                    //Parse length prefix
                    length = int.Parse(netstring.Substring(0, i));

                    //Make sure seperator present
                    if (!(netstring[i] == ':'))
                        throw new ArgumentException("Netstring does not have a seperator between the length field and content");

                    //Trim off length prefix
                    json = netstring.Substring(i + 1);

                    //Trim string based on length
                    json = json.Substring(0, length);

                    break;
                }                
            }

            if (json == null)
                throw new Exception("Tried to parse invalid netstring: " + netstring);

            //Check length
            if (json.Length != length)
                throw new Exception("Netstring body does not match length prefix: " + netstring);

            return json;
        }
    }
}
