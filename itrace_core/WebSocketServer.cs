﻿using iTrace_Core.Properties;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace iTrace_Core
{
    class WebSocketServer
    {
        TcpListener server;
        List<WebSocket> clients;
        BlockingCollection<WebSocket> clientAcceptQueue;

        const string localhostAddress = "127.0.0.1";
        const int defaultPort = 7007;
        public const int MIN_WEBSOCKET_PORT_NUM = 1025;
        public const int MAX_WEBSOCKET_PORT_NUM = 65535;
        int port;
        
        public WebSocketServer()
        {
            try
            {
                clients = new List<WebSocket>();
                clientAcceptQueue = new BlockingCollection<WebSocket>();

                port = Settings.Default.websocket_port;

                server = new TcpListener(IPAddress.Parse(localhostAddress), port);
                server.Start();

                GazeHandler.Instance.OnGazeDataReceived += ReceiveGazeData;

                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    AcceptIncomingWebsocketConnections();
                }).Start();
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode.Equals(SocketError.AddressAlreadyInUse))
                {
                    System.Console.WriteLine("Web");
                    MessageBox.Show("Another service is running on port " + port + ".\nStop that service or change the port for iTrace Core in settings and restart the Core.", "Websocket Server Cannot Start", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        void AcceptIncomingWebsocketConnections()
        {
            while (true)
            {
                WebSocket ws = new WebSocket(server.AcceptTcpClient());

                new Thread(() =>
                {
                    if (ws.PerformHandshake(10000))
                    {
                        if (SessionManager.GetInstance().Active)
                        {
                            ws.SendMessage(SessionManager.GetInstance().Serialize());
                        }

                        clientAcceptQueue.Add(ws);
                    }
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
            while (clientAcceptQueue.TryTake(out var acceptedClient))
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
            SendToClients(SessionManager.GetInstance().Serialize());
        }

        public void SendEndSession()
        {
            SendToClients("session_stop\n");
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
