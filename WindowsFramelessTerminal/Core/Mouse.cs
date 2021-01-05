using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFramelessTerminal.Core
{
    internal class CoreMouse
    {
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Graphics.Point lpPoint);

        /// <summary>
        /// Fetches the cursor position.
        /// </summary>
        /// <returns>Graphics.Pointed struct-ed</returns>
        public static System.Drawing.Point GetCursorPosition()
        {
            Graphics.Point lpPoint;
            GetCursorPos(out lpPoint);
            return lpPoint;
        }

    }
}
