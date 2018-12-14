using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    class SocketServer
    {
        public void Start()
        {
            GazeHandler.Instance.OnGazeDataReceived += ReceiveGazeData;            
        }

        private void SendToClients(string message)
        {

        }

        private void ReceiveGazeData(object sender, GazeDataReceivedEventArgs e)
        {
            SendToClients(e.ReceivedGazeData.Serialize());
        }
    }
}
