using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;


namespace DejaVuLib
{
    public class WebSocketServer
    {
        List<WebSocket> clients;
        BlockingCollection<WebSocket> clientAcceptQueue;

        const string localhostAddress = "127.0.0.1";
        const int defaultPort = 7007;

        static WebSocketServer instance;
        public static WebSocketServer Instance()
        {
            if (instance == null)
                instance = new WebSocketServer();

            return instance;
        }

        private WebSocketServer()
        {
            clients = new List<WebSocket>();
            clientAcceptQueue = new BlockingCollection<WebSocket>();

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
                ws.WaitForConnection(localhostAddress, defaultPort);

                new Thread(() =>
                {
                    if (ws.PerformHandshake(10000))
                    {
                        clientAcceptQueue.Add(ws);
                    }
                }).Start();
            }
        }

        public void Send(string message)
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
    }
}
