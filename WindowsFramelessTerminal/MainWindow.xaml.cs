using System;
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
            
        public static IntPtr hWnd = WindowsAPI.FindWindow("mintty", null);
        public static bool isDraggingWindow = false;
        public static System.Drawing.Rectangle CurrentWindowRectangle;

        public static void WindowsReStyle()
        {
            Process[] Procs = Process.GetProcesses();
            foreach (Process proc in Procs)
            {
                if (proc.ProcessName.StartsWith("mintty"))
                {
                    IntPtr pFoundWindow = proc.MainWindowHandle;
                    int style = WindowsAPI.GetWindowLong(pFoundWindow, WindowsAPI.GWL_STYLE);
                    WindowsAPI.SetWindowLong(pFoundWindow, WindowsAPI.GWL_STYLE, (style & ~WindowsAPI.WS_CAPTION));
                }
            }
        }
        static void Loop()
        {
            System.Drawing.Rectangle win;
            WindowsAPI.GetWindowRect(hWnd, out win);

            while (true)
            {
                IntPtr currentWindow = WindowsAPI.GetForegroundWindow();

                if (isDraggingWindow && currentWindow == hWnd)
                {
                    System.Drawing.Point xandy = WindowsAPI.GetCursorPosition();
                    WindowsAPI.MoveWindow(hWnd, xandy.X, xandy.Y, win.Width, win.Height, true);
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

            IntPtr hWnd = WindowsAPI.FindWindow("mintty", null);

            System.Drawing.Rectangle test;
            WindowsAPI.GetWindowRect(hWnd, out test);

            WindowsAPI.SetWindowPos(hWnd, 0, test.X, test.Y, test.Width, test.Height-1, 0);

            if (hWnd != IntPtr.Zero)
            {
                Thread loop = new Thread(Loop);
                loop.Start();
            }

            StartWatchBtn.IsEnabled = false;

            uint processId;
            WindowsAPI.GetWindowThreadProcessId(hWnd, out processId);
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
