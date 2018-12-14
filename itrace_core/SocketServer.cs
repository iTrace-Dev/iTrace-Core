using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace iTrace_Core
{
    class SocketServer
    {
        TcpListener server;
        List<TcpClient> clients;
        BlockingCollection<TcpClient> clientAcceptQueue;
        Thread connectionsListener;

        public SocketServer()
        {
            clients = new List<TcpClient>();
            clientAcceptQueue = new BlockingCollection<TcpClient>();

            server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8008);
            server.Start();

            connectionsListener = new Thread(new ThreadStart(() => {
                Thread.CurrentThread.IsBackground = true;
                ListenForConnections();
            }));
            connectionsListener.Start();

            GazeHandler.Instance.OnGazeDataReceived += ReceiveGazeData;
        } 

        private void ListenForConnections()
        {
            TcpClient client;

            while(true)
            {
                client = server.AcceptTcpClient();
                clientAcceptQueue.Add(client);

                Console.WriteLine("Client connected!");
            }
        }

        private void AcceptQueuedClients()
        {
            int clientQueueCount = clientAcceptQueue.Count;

            for(int i = clientQueueCount; i != 0; --i)
            {
                TcpClient client = clientAcceptQueue.Take();
                clients.Add(client);
            }
        }

        private void SendToClients(string message)
        {
            byte[] messageInBytes = Encoding.ASCII.GetBytes(message);

            for(int i = clients.Count - 1; i >= 0; i--)
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

        private void ReceiveGazeData(object sender, GazeDataReceivedEventArgs e)
        {
            AcceptQueuedClients();
            SendToClients(e.ReceivedGazeData.Serialize());
        }
    }
}
