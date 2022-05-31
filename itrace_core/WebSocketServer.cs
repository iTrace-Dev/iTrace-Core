/********************************************************************************************************************************************************
* @file WebSocketServer.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

using iTrace_Core.Properties;
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
        
        private WebSocketServer()
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
                    string content = Resources.AnotherServiceIsRunningOnPort + port + '\n' + Resources.StopServiceOrChangePortThenRestart;
                    string title = Resources.WebSocketServerCannotStart;
                    MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        static WebSocketServer instance;
        public static WebSocketServer Instance()
        {
            if (instance == null)
                instance = new WebSocketServer();
            return instance;
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

        public void SendToClients(string message)
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
