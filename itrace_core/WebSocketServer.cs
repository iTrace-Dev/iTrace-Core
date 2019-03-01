using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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
            while (true)
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
            for (int i = clients.Count - 1; i >= 0; --i)
            {
                if (!clients[i].Connected)
                {
                    clients.RemoveAt(i);
                }
            }

            //Accept queued clients
            WebSocket acceptedClient;
            while (clientAcceptQueue.TryTake(out acceptedClient))
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

        public void SendSessionData()
        {
            string data = "session," + SessionManager.GetInstance().DataRootDir + '\n';
            SendToClients(data);
        }

        private void ReceiveGazeData(object sender, GazeDataReceivedEventArgs e)
        {
            if (e.ReceivedGazeData.IsValid())
            {
                SendToClients(e.ReceivedGazeData.Serialize());
            }
        }
    }
}
