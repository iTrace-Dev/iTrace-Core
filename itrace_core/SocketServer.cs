using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        CancellationTokenSource cancellationTokenSource;

        const string localhostAddress = "127.0.0.1";
        const int defaultPort = 8008;
        public const int MIN_PORT_NUM = 1025;
        public const int MAX_PORT_NUM = 65535;
        int port;


        public bool Started { get; private set; }

        private SocketServer()
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
                    string content = Resources.AnotherServiceIsRunningOnPort + port + ".\n" + Resources.StopServiceOrChangePortThenRestart;
                    string title = Resources.SocketServerCannotStart;
                    MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Started = false;
            }
        }

        static SocketServer instance;
        public static SocketServer Instance()
        {
            if (instance == null)
                instance = new SocketServer();
            return instance;
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

        public void SendToClients(string message)
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

        private async Task WaitForMessageFromClient(int clientIndex, CancellationToken cancellationToken, int timeout)
        {
            const int sleepLength = 10;
            int slept = 0;
            byte[] buffer = new byte[8];

            try
            {
                NetworkStream networkStream = clients[clientIndex].GetStream();
                //clients[clientIndex].Client.Poll(1000, SelectMode.SelectRead);

                while (true)
                {
                    if (cancellationToken.IsCancellationRequested || slept > timeout)
                    {
                        return;
                    }

                    if (networkStream.DataAvailable)
                    {
                        networkStream.Read(buffer, 0, buffer.Length);

                        return;
                    }

                    Thread.Sleep(sleepLength);
                    slept += sleepLength;
                }
            }
            catch (System.IO.IOException) { }    // Disconnect will be handled by SendToClients

            return;
        }

        public void WaitUntilClientsAreReady(int timeoutMilliseconds)
        {
            int clientCount = clients.Count;
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < clientCount; ++i)
            {
                var clientTask = WaitForMessageFromClient(i, cancellationTokenSource.Token, timeoutMilliseconds);
                tasks.Add(clientTask);
            }

            Task.WhenAll(tasks).Wait();

            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
        }

        public void CancelWait()
        {
            cancellationTokenSource.Cancel();
        }
    }
}