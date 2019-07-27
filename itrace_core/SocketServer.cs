using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using iTrace_Core.Properties;
using System.Windows;

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
        public const int MIN_PORT_NUM = 1025;
        public const int MAX_PORT_NUM = 65535;
        int port;

        public bool Started { get; private set; }

        public SocketServer()
        {
            try
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

                Started = true;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode.Equals(SocketError.AddressAlreadyInUse))
                {
                    string content = "Another service is running on port " + port +
                                     ".\nStop that service or change the port for iTrace Core in settings and restart the Core.";
                    string title = "Socket Server Cannot Start";
                    MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Started = false;
            }
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
            if (!Started)
                return;

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
            if (e.ReceivedGazeData.IsValid() && Started)
            {
                AcceptQueuedClients();

                SendToClients(e.ReceivedGazeData.Serialize());
            }
        }
    }
}
