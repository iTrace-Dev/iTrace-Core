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

        //Returns value from key if it exists. Otherwise, returns the default value provided. 
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

        public void WriteConfiguration(string key, string value)
        {
            configurations[key] = value;

            Configuration config = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);

            config.AppSettings.Settings.Remove(key);
            config.AppSettings.Settings.Add(key, value);

            config.Save(ConfigurationSaveMode.Modified);
        }
    }
}
