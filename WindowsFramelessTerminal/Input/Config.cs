using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WindowsFramelessTerminal.Input.Models;

namespace WindowsFramelessTerminal.Input
{
    class Config
    {
        private static string fileName = "config.json";

        public static void ParseConfig()
        {
            var configContents = GetConfigContents();

            if(configContents.Equals(String.Empty))
            {
                MessageBox.Show("WFT cannot work without config.json file!\n\n" + 
                    "Please follow documentation and add one before starting the app. You can visit the GitHub repository for a sample configuration file.", "Configuration file missing!", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
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
