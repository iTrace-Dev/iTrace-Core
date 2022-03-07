using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace iTrace_Core
{
    public class CoreClient
    {
        public event EventHandler<ComputerEvent> OnCoreMessage;

        const string localhostAddress = "127.0.0.1";
        const int port = 8008;

        TcpClient tcpClient;
        NetworkStream networkStream;
        StreamReader streamReader;

        Thread listener;

        public CoreClient()
        {
            tcpClient = new TcpClient();
        }

        public void Connect()
        {
            tcpClient.Connect(localhostAddress, port);
            networkStream = tcpClient.GetStream();
            streamReader = new StreamReader(networkStream);
            listener = new Thread(Listen);
            listener.IsBackground = true;
            listener.Start();
        }

        public void Disconnect()
        {
            listener.Abort();
        }

        public void Listen()
        {
            try
            {
                while (true)
                {
                    string text = streamReader.ReadLine();
                    OnCoreMessage(this, new CoreMessage(text));
                }
            }
            catch (ThreadAbortException e)
            {

            }
            catch (IOException e)
            {

            }
        }
    }
}
