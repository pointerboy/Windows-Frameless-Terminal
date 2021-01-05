using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFramelessTerminal.Core
{
    class Graphics
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;

            /// <summary>
            /// Returns a System Drawing Point from given co-ords.
            /// </summary>
            /// <param name="point"></param>
            public static implicit operator System.Drawing.Point(Point point)
            {
                return new System.Drawing.Point(point.X, point.Y);
            }
        }
    }
}
