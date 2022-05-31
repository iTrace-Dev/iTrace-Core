/********************************************************************************************************************************************************
* @file CoreClient.cs
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
