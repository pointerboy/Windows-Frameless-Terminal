using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFramelessTerminal
{
    class Config
    {
        private static string fileName = "config.json";

        public static void ParseConfig()
        {
            var parsed = JsonConvert.DeserializeObject<ConfigData>(GetConfigContents());
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
