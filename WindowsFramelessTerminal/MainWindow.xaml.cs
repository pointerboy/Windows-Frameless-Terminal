﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WindowsFramelessTerminal
{
    public partial class MainWindow : Window
    {
            
        public static IntPtr hWnd = FindWindow("mintty", null);
        public static bool isDraggingWindow = false;
        public static System.Drawing.Rectangle CurrentWindowRectangle;

        [DllImport("USER32.DLL")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        //Sets a window to be a child window of another window
        [DllImport("USER32.DLL")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        //Sets window attributes
        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        //Gets window attributes
        [DllImport("USER32.DLL")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);


        //assorted constants needed
        public static int GWL_STYLE = -16;
        public static int WS_CHILD = 0x40000000; //child window
        public static int WS_BORDER = 0x00800000; //window with border
        public static int WS_DLGFRAME = 0x00400000; //window with double border but no title
        public static int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar

        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOZORDER = 0x0004;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();


        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public static void WindowsReStyle()
        {
            Process[] Procs = Process.GetProcesses();
            foreach (Process proc in Procs)
            {
                if (proc.ProcessName.StartsWith("mintty"))
                {
                    IntPtr pFoundWindow = proc.MainWindowHandle;
                    int style = GetWindowLong(pFoundWindow, GWL_STYLE);
                    SetWindowLong(pFoundWindow, GWL_STYLE, (style & ~WS_CAPTION));
                }
            }
        }

        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator System.Drawing.Point(POINT point)
            {
                return new System.Drawing.Point(point.X, point.Y);
            }
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd, out System.Drawing.Rectangle rect);

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")]
        static extern bool EnableWindow(IntPtr hWnd, bool enable);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        public static System.Drawing.Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            return lpPoint;
        }

        static void Loop()
        {
            System.Drawing.Rectangle win;
            GetWindowRect(hWnd, out win);

            while (true)
            {
                IntPtr currentWindow = GetForegroundWindow();

                if (isDraggingWindow && currentWindow == hWnd)
                {
                    System.Drawing.Point xandy = GetCursorPosition();
                    MoveWindow(hWnd, xandy.X, xandy.Y, win.Width, win.Height, true);
                }
            }
        }
        KeyboardListener KListener = new KeyboardListener();
        public MainWindow()
        {
            KListener.KeyDown += new RawKeyEventHandler(KListener_KeyDown);
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowsReStyle();

            IntPtr hWnd = FindWindow("mintty", null);

            System.Drawing.Rectangle test;
            GetWindowRect(hWnd, out test);

            SetWindowPos(hWnd, 0, test.X, test.Y, test.Width, test.Height-1, 0);

            if (hWnd != IntPtr.Zero)
            {
                Thread loop = new Thread(Loop);
                loop.Start();
            }

            StartWatchBtn.IsEnabled = false;

            uint processId;
            GetWindowThreadProcessId(hWnd, out processId);
            InfoLbl.Content = "Info: PID: " + Convert.ToString(processId);
        }

        void KListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            if(args.Key.ToString() == "LeftAlt")
            {
                isDraggingWindow = !isDraggingWindow;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            KListener.Dispose();
        }

    }
}
