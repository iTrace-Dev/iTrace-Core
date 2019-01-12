using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace iTrace_Core
{
    class WebSocketServer
    {
        TcpListener server;

        const string localhostAddress = "127.0.0.1";
        const int defaultPort = 7007;
        int port;

        public WebSocketServer()
        {
            port = ConfigurationRegistry.Instance.AssignFromConfiguration("websocket_port", defaultPort);

            WebSocket ws = new WebSocket(localhostAddress, port);
            ws.SendMessage("good day!");

        }
    }
}
