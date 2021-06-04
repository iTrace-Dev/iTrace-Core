using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;


namespace DejaVuLib
{
    class WebSocket
    {
        TcpClient client;
        NetworkStream stream;

        public bool Connected { get; private set; }

        public WebSocket() { }

        public void WaitForConnection(string address, int port)
        {
            TcpListener server = new TcpListener(IPAddress.Parse(address), port);
            server.Start();

            client = server.AcceptTcpClient();
            stream = client.GetStream();

            server.Stop();
        }

        //Returns true if succeeded, false if failed.
        public bool PerformHandshake(long timeout)
        {
            long timeoutCountDown = 0;

            //Wait for a response
            while (client.Available < 3)
            {
                const int updateLength = 100;

                Thread.Sleep(updateLength);
                timeoutCountDown += updateLength;

                if (timeoutCountDown > timeout)
                {
                    return false;
                }
            }

            byte[] request = new byte[client.Available];
            stream.Read(request, 0, request.Length);
            string requestString = Encoding.UTF8.GetString(request);

            if (Regex.IsMatch(requestString, "^GET"))
            {
                const string eol = "\r\n";

                byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + eol
                    + "Connection: Upgrade" + eol
                    + "Upgrade: websocket" + eol
                    + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                        System.Security.Cryptography.SHA1.Create().ComputeHash(
                            Encoding.UTF8.GetBytes(
                                new Regex("Sec-WebSocket-Key: (.*)").Match(requestString).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                            )
                        )
                    ) + eol
                    + eol);

                stream.Write(response, 0, response.Length);


                Connected = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SendMessage(string message)
        {
            if (!Connected)
            {
                return;
                //throw new WebSocketUnconnectedException();
            }

            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] frame = new byte[2 + messageBytes.Length];


            //Whole message sent in this frame, no extensions, Opcode type is text message
            frame[0] = 129;

            //Length of message, with mask bit set to 0. Max message length is 128 characters
            frame[1] = Convert.ToByte(messageBytes.Length);

            //Masked message
            for (int i = 0; i < messageBytes.Length; ++i)
            {
                frame[2 + i] = Convert.ToByte(messageBytes[i]);
            }

            try
            {
                stream.Write(frame, 0, frame.Length);
            }
            catch (IOException e)
            {
                Connected = false;
            }
        }
    }
}
