using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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

            port = ConfigurationRegistry.Instance.AssignFromConfiguration("socket_port", defaultPort);
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
            string data = "session," + SessionManager.GetInstance().DataRootDir + '\n';
            SendToClients(data);
        }

        private void ListenForConnections()
        {
            TcpClient client;

            while (true)
            {
                client = server.AcceptTcpClient();
                clientAcceptQueue.Add(client);

                Console.WriteLine("Client connected!");
            }
        }

        private void AcceptQueuedClients()
        {
            int clientQueueCount = clientAcceptQueue.Count;

            for (int i = clientQueueCount; i != 0; --i)
            {
                TcpClient client = clientAcceptQueue.Take();
                clients.Add(client);
            }
        }

        private void SendToClients(string message)
        {
            AcceptQueuedClients();

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
                SendToClients(e.ReceivedGazeData.Serialize());
            }
        }
    }
}
