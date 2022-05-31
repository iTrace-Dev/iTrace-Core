/********************************************************************************************************************************************************
* @file WebSocket.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace iTrace_Core
{
    class WebSocket
    {
        TcpClient client;
        NetworkStream stream;
        
        public bool Connected { get; private set; }

        public WebSocket(TcpClient connection)
        {
            client = connection;
            stream = client.GetStream();
        }

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
            int websocketFrameOffset = 2 ;
            if (messageBytes.Length > 125)
            {
                if (messageBytes.Length <= 65535)
                {
                    websocketFrameOffset += 2;
                }
                else
                {
                    websocketFrameOffset += 8;
                }
            }
            byte[] frame = new byte[websocketFrameOffset + messageBytes.Length];


            //Whole message sent in this frame, no extensions, Opcode type is text message
            frame[0] = 129;

            //Length of message, with mask bit set to 0.
            // bottom 7 bits are length if length is less than 125
            if (messageBytes.Length <= 125)
            {
                frame[1] = Convert.ToByte(messageBytes.Length);
            }
            // bottom 7 bits are 126, the next two bytes are the length
            else if (messageBytes.Length <= 65535)
            {
                frame[1] = Convert.ToByte(126);
                frame[2] = Convert.ToByte((messageBytes.Length & 0xff00) >> 8);
                frame[3] = Convert.ToByte(messageBytes.Length & 0x00ff);
            }
            // bottom 7 bits are 127, the next 8 bytes are the length
            else
            {
                frame[1] = Convert.ToByte(127);
                frame[2] = Convert.ToByte(0);
                frame[3] = Convert.ToByte(0);
                frame[4] = Convert.ToByte(0);
                frame[5] = Convert.ToByte(0);
                frame[6] = Convert.ToByte((messageBytes.Length & 0xff000000) >> 24);
                frame[7] = Convert.ToByte((messageBytes.Length & 0x00ff0000) >> 16);
                frame[8] = Convert.ToByte((messageBytes.Length & 0x0000ff00) >> 8);
                frame[9] = Convert.ToByte(messageBytes.Length & 0x000000ff);
            }


            //Masked message
            for (int i = 0; i < messageBytes.Length; ++i)
            {
                frame[websocketFrameOffset + i] = Convert.ToByte(messageBytes[i]);
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

    public class WebSocketUnconnectedException : Exception
    {
        public WebSocketUnconnectedException()
        {

        }

        public WebSocketUnconnectedException(string message)
            : base(message)
        {
        }

        public WebSocketUnconnectedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
