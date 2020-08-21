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
        public static IntPtr WindowPointer;
        public static bool isDraggingWindow = false;
        public static System.Drawing.Rectangle CurrentWindowRectangle;

        public static bool ui_FoundProcess;

        private Thread windowManagerThread = new Thread(WindowManager);

        private void UI_RefreshSettings()
        {
            ProcessLabel.Content = "Process name: " + ConfigData.processName;
            MoveKeyLbl.Content = "Move key: " + ConfigData.moveKey;

            StartWatchBtn.IsEnabled = false;
        }

        public void StartWatch()
        {
            WindowsReStyle();

            if (!ui_FoundProcess)
            {
                MessageBox.Show("Could not find the process " + ConfigData.processName,
                    "Windows Frameless Terminal", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }

            WindowPointer = WindowsAPI.FindWindow(ConfigData.processName, null);

            WindowsAPI.GetWindowRect(WindowPointer, out CurrentWindowRectangle);

            WindowsAPI.SetWindowPos(WindowPointer, 0, CurrentWindowRectangle.X, CurrentWindowRectangle.Y,
                CurrentWindowRectangle.Width, CurrentWindowRectangle.Height - 1, 0);

            if (windowManagerThread.IsAlive) windowManagerThread.Abort();

            if (WindowPointer != IntPtr.Zero) windowManagerThread.Start();

            StartWatchBtn.IsEnabled = false;

            uint processId;
            WindowsAPI.GetWindowThreadProcessId(WindowPointer, out processId);
            InfoLbl.Content = "Info: PID: " + Convert.ToString(processId);
        }

        public static void WindowsReStyle()
        {
            Process[] Procs = Process.GetProcesses();
            foreach (Process proc in Procs)
            {
                if (proc.ProcessName.StartsWith(ConfigData.processName))
                {
                    IntPtr pFoundWindow = proc.MainWindowHandle;
                    int style = WindowsAPI.GetWindowLong(pFoundWindow, WindowsAPI.GWL_STYLE);
                    WindowsAPI.SetWindowLong(pFoundWindow, WindowsAPI.GWL_STYLE, (style & ~WindowsAPI.WS_CAPTION));

                    ui_FoundProcess = true;
                }
            }
        }
        static void WindowManager()
        {
            System.Drawing.Rectangle originalWindowRect;
            WindowsAPI.GetWindowRect(WindowPointer, out originalWindowRect);

            Console.WriteLine("Original Window: " +  originalWindowRect);

            while (true)
            {
                IntPtr currentWindow = WindowsAPI.GetForegroundWindow();

                if (isDraggingWindow && currentWindow == WindowPointer)
                {
                    System.Drawing.Point xandy = WindowsAPI.GetCursorPosition();
                    
                    WindowsAPI.MoveWindow(WindowPointer, xandy.X, xandy.Y, 600, 700, true);
                    Console.WriteLine(originalWindowRect);
                    Console.WriteLine(xandy);
                }
            }
        }

        KeyboardListener KListener = new KeyboardListener();
        public MainWindow()
        {
            KListener.KeyDown += new RawKeyEventHandler(KListener_KeyDown);
            Config.ParseConfig();

            InitializeComponent();


            ProcessLabel.Content = "Process name: " + ConfigData.processName;
            MoveKeyLbl.Content += ConfigData.moveKey;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowsReStyle();
            StartWatch();
        }

        void KListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            if(args.Key.ToString() == ConfigData.moveKey)
            {
                isDraggingWindow = !isDraggingWindow;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            KListener.Dispose();
        }

        private void ReloadSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            Config.ParseConfig();
            UI_RefreshSettings();
            StartWatch();
        }

    }
}
