using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace iTrace_Core
{
    class WebSocketServer
    {
        WebSocket ws;

        const string localhostAddress = "127.0.0.1";
        const int defaultPort = 7007;
        int port;

        public WebSocketServer()
        {
            port = ConfigurationRegistry.Instance.AssignFromConfiguration("websocket_port", defaultPort);

            ws = new WebSocket(localhostAddress, port);

            GazeHandler.Instance.OnGazeDataReceived += ReceiveGazeData;
        }
        
        void SendToClients(string message)
        {
            ws.SendMessage(message);    //Todo: multiple clients
        }

        public void SendSessionData(SessionManager s)
        {
            string data = "session," + s.DataRootDir + @"\\" + s.ResearcherName + @"\\" + s.ParticipantID + @"\\" + s.CurrentSessionID + "\n";
            SendToClients(data);
        }

        private void ReceiveGazeData(object sender, GazeDataReceivedEventArgs e)
        {
            SendToClients(e.ReceivedGazeData.Serialize());
        }
    }
}
