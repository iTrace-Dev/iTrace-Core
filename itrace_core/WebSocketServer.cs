using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Generic;

namespace iTrace_Core
{
    class WebSocketServer
    {
        /*
        List<WebSocket> clients;
        */
        WebSocket ws;

        const string localhostAddress = "127.0.0.1";
        const int defaultPort = 7007;
        int port;

        public WebSocketServer()
        {
            //clients = new List<WebSocket>();
            ws = new WebSocket();

            port = ConfigurationRegistry.Instance.AssignFromConfiguration("websocket_port", defaultPort);

            GazeHandler.Instance.OnGazeDataReceived += ReceiveGazeData;

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                ws.WaitForConnection(localhostAddress, port);
                ws.PerformHandshake(10000);
            }).Start();
        }
        
        void SendToClients(string message)
        {
            if (ws.Connected)
            {
                ws.SendMessage(message);    //Todo: multiple clients
            }
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
