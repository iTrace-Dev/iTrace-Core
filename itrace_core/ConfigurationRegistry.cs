using System;
using System.Configuration;

namespace iTrace_Core
{
    sealed class ConfigurationRegistry
    {
        private static readonly Lazy<ConfigurationRegistry> Singleton = 
            new Lazy<ConfigurationRegistry>(() => new ConfigurationRegistry());

        public static ConfigurationRegistry Instance { get { return Singleton.Value; } }

        public int SocketPort { get; private set; }
        private const int defaultSocketPort = 8008;

        private ConfigurationRegistry() 
        {
            SocketPort = Convert.ToInt32(ConfigurationManager.AppSettings["sockets_port"]);
              
            if (SocketPort == 0) 
                SocketPort = defaultSocketPort;
        }
    }
}
