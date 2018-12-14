using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace iTrace_Core
{
    class SocketServer
    {
        TcpListener server;
        List<TcpClient> clients;

        public SocketServer()
        {
            clients = new List<TcpClient>();

            server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8008);
        }

        public void Start()
        {
            server.Start();

            TcpClient client = server.AcceptTcpClient();
            clients.Add(client);

            Console.WriteLine("Client connected!");

            GazeHandler.Instance.OnGazeDataReceived += ReceiveGazeData;
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
            SendToClients(e.ReceivedGazeData.Serialize());
        }
    }
}
