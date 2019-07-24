﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using iTrace_Core.Properties;

namespace iTrace_Core
{
    class SocketServer
    {
        TcpListener server;
        List<TcpClient> clients;
        BlockingCollection<TcpClient> clientAcceptQueue;
        Thread connectionsListener;

        const string localhostAddress = "127.0.0.1";
        const int defaultPort = 8008;
        int port;

        public SocketServer()
        {
            clients = new List<TcpClient>();
            clientAcceptQueue = new BlockingCollection<TcpClient>();

            port = Settings.Default.socket_port;
            server = new TcpListener(IPAddress.Parse(localhostAddress), port);
            server.Start();

            connectionsListener = new Thread(new ThreadStart(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                ListenForConnections();
            }));
            connectionsListener.Start();

            GazeHandler.Instance.OnGazeDataReceived += ReceiveGazeData;
        }

        public void SendSessionData()
        {
            SendToClients(SessionManager.GetInstance().Serialize());
        }

        public void SendSessionData(TcpClient client)
        {
            SendToClient(client, SessionManager.GetInstance().Serialize());
        }

        public void SendEndSession()
        {
            SendToClients("session_end\n");
        }

        private void ListenForConnections()
        {
            TcpClient client;

            while (true)
            {
                client = server.AcceptTcpClient();
                clientAcceptQueue.Add(client);
            }
        }

        private void AcceptQueuedClients()
        {
            int clientQueueCount = clientAcceptQueue.Count;

            for (int i = clientQueueCount; i != 0; --i)
            {
                TcpClient client = clientAcceptQueue.Take();

                if (SessionManager.GetInstance().Active)
                {
                    SendSessionData(client);
                }

                clients.Add(client);
            }
        }

        private void SendToClients(string message)
        {
            byte[] messageInBytes = Encoding.ASCII.GetBytes(message);

            for (int i = clients.Count - 1; i >= 0; i--)
            {
                try
                {
                    clients[i].GetStream().Write(messageInBytes, 0, messageInBytes.Length);
                }
                catch (System.IO.IOException e)
                {   //client was disconnected
                    clients.RemoveAt(i);
                }
                catch (InvalidOperationException e)
                {
                    clients.RemoveAt(i);
                }
            }
        }

        private void SendToClient(TcpClient client, string message)
        {
            byte[] messageInBytes = Encoding.ASCII.GetBytes(message);

            try
            {
                client.GetStream().Write(messageInBytes, 0, messageInBytes.Length);
            }
            catch (System.IO.IOException e)
            {

            }
        }

        private void ReceiveGazeData(object sender, GazeDataReceivedEventArgs e)
        {
            if (e.ReceivedGazeData.IsValid())
            {
                AcceptQueuedClients();

                SendToClients(e.ReceivedGazeData.Serialize());
            }
        }
    }
}
