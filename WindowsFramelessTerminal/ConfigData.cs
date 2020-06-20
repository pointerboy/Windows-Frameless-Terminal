using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFramelessTerminal
{
    [DataContract]
    class ConfigData
    {
        [DataMember] public static string process_name { get; set; }
    }
}
