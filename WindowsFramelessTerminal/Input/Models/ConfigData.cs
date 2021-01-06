using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFramelessTerminal.Input.Models
{
    [DataContract]
    internal class  ConfigData
    {
        [DataMember] public static string ProcessName { get; set; }
        [DataMember] public static string MoveKey { get; set; }

        [DataMember] public static int StaticWidth { get; set; }
        [DataMember] public static int StaticHeight { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false) ] public static string HighlighterColor { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)] public static int HighlighterBorderWidth { get;set; }
    }
}
