using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DejaVuLib
{
    public class SocketServer
    {
        TcpListener server;
        List<TcpClient> clients;
        BlockingCollection<TcpClient> clientAcceptQueue;
        Thread connectionsListener;
        CancellationTokenSource cancellationTokenSource;

        const string localhostAddress = "127.0.0.1";
        const int port = 8008;

        private SocketServer()
        {
            clients = new List<TcpClient>();
            clientAcceptQueue = new BlockingCollection<TcpClient>();

            server = new TcpListener(IPAddress.Parse(localhostAddress), port);
            server.Start();

            connectionsListener = new Thread(new ThreadStart(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                ListenForConnections();
            }));
            connectionsListener.Start();

            cancellationTokenSource = new CancellationTokenSource();
        }

        static SocketServer instance;
        public static SocketServer Instance()
        {
            if (instance == null)
                instance = new SocketServer();

            return instance;
        }

        private void ListenForConnections()
        {
            while (true)
            {
                var client = server.AcceptTcpClient();
                clientAcceptQueue.Add(client);
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

        public void SendToClients(string message)
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
                catch (InvalidOperationException e)
                {
                    clients.RemoveAt(i);
                }
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
