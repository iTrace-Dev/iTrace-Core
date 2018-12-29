using System;
using System.Collections.Generic;
using System.Configuration;

namespace iTrace_Core
{
    sealed class ConfigurationRegistry
    {
        private static readonly Lazy<ConfigurationRegistry> Singleton = 
            new Lazy<ConfigurationRegistry>(() => new ConfigurationRegistry());

        public static ConfigurationRegistry Instance { get { return Singleton.Value; } }

        private Dictionary<string, string> configurations;

        private ConfigurationRegistry() 
        {
            configurations = new Dictionary<string, string>();
            
            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                configurations[key] = ConfigurationManager.AppSettings[key];
            }
        }

        public string AssignFromConfiguration(string key, string defaultValue)
        {
            if (configurations.ContainsKey(key))
                return configurations[key];

            return defaultValue;
        }

        public int AssignFromConfiguration(string key, int defaultValue)
        {
            if (configurations.ContainsKey(key))
                return Convert.ToInt32(configurations[key]);

            return defaultValue;
        }
    }
}
