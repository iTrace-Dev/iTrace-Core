using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace iTrace_Core
{
    class WebSocketServer
    {
        List<WebSocket> clients;
        BlockingCollection<WebSocket> clientAcceptQueue;

        const string localhostAddress = "127.0.0.1";
        const int defaultPort = 7007;
        int port;

        public WebSocketServer()
        {
            clients = new List<WebSocket>();
            clientAcceptQueue = new BlockingCollection<WebSocket>();

            port = ConfigurationRegistry.Instance.AssignFromConfiguration("websocket_port", defaultPort);

            GazeHandler.Instance.OnGazeDataReceived += ReceiveGazeData;

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                AcceptIncomingWebsocketConnections();
            }).Start();
        }

        void AcceptIncomingWebsocketConnections()
        {
            while(true)
            {
                WebSocket ws = new WebSocket();
                ws.WaitForConnection(localhostAddress, port);

                new Thread(() =>
                {
                    if (ws.PerformHandshake(10000))
                        clientAcceptQueue.Add(ws);
                }).Start();
            }
        }
        
        void SendToClients(string message)
        {
            //Todo: Remove disconnected clients

            //Accept queued clients
            WebSocket acceptedClient;
            while(clientAcceptQueue.TryTake(out acceptedClient))
            {
                clients.Add(acceptedClient);
            }

            //Send Message to all accepted clients
            foreach (WebSocket ws in clients)
            {
                if (ws.Connected)
                {
                    ws.SendMessage(message);
                }
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
