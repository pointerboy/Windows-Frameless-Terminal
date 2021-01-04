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
        [DataMember] public static string processName { get; set; }
        [DataMember] public static string moveKey { get; set; }

        [DataMember] public static int staticWidth { get; set; }
        [DataMember] public static int staticHeight { get; set; }
    }
}
