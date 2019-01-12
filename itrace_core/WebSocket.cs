using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace iTrace_Core
{
    class WebSocket
    {
        TcpListener server;
        TcpClient client;
        NetworkStream stream;
        
        byte[] mask = new byte[4] { 16, 68, 42, 10 };

        public WebSocket(string address, int port)
        {
            server = new TcpListener(IPAddress.Parse(address), port);
            server.Start();

            client = server.AcceptTcpClient();

            stream = client.GetStream();

            while (client.Available < 3) { }

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
            }
            else
            {

            }
        }

        public void SendMessage(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] frame = new byte[6 + messageBytes.Length];


            //Whole message sent in this frame, no extensions, Opcode type is text message
            frame[0] = 129;

            //Length of message, with mask bit set to 1. Max message length is 128 characters
            frame[1] = Convert.ToByte(128 + messageBytes.Length);

            //Mask
            frame[2] = mask[0];
            frame[3] = mask[1];
            frame[4] = mask[2];
            frame[5] = mask[3];

            //Masked message
            for (int i = 0; i < messageBytes.Length; ++i)
            {
                frame[6 + i] = Convert.ToByte(messageBytes[i] ^ mask[i % 4]);
            }
            
            stream.Write(frame, 0, frame.Length);
        }
    }
}
