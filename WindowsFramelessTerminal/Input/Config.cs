using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WindowsFramelessTerminal
{
    class Config
    {
        private static string fileName = "config.json";

        public static void ParseConfig()
        {
            var configContents = GetConfigContents();

            if(configContents.Equals(String.Empty))
            {
                MessageBox.Show("Configuration file (config.json) is " +
                    "missing from local directory. Please follow documentation and add one before starting the app.", "Configuration file missing", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            JsonConvert.DeserializeObject<ConfigData>(GetConfigContents());
        }

        /// <summary>
        /// Reading from config file(def:configjson)
        /// </summary>
        /// <returns>File contents as a string</returns>
        public static string GetConfigContents()
        {
            if (!File.Exists(fileName))
                return "";

            return File.ReadAllText(fileName);
        }
    }
}
