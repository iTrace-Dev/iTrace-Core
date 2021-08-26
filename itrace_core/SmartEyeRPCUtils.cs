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
    }
}
