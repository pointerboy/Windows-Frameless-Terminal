using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Media;
using WindowsFramelessTerminal.Core;
using WindowsFramelessTerminal.Input;
using WindowsFramelessTerminal.Input.Models;

namespace WindowsFramelessTerminal
{
    public partial class MainWindow : Window
    {
        public static IntPtr WindowPointer;
        public static bool IsDraggingWindow = false;
        public static System.Drawing.Rectangle CurrentWindowRectangle;

        public static bool UiFoundProcess;

        private readonly Thread windowManagerThread = new Thread(WindowManager);

        readonly KeyboardListener keyboardListener = new KeyboardListener();

        private void UI_PopulateSettings()
        {
            mainListView.Items.Clear();
            mainListView.Items.Add("Process Name:  " + ConfigData.ProcessName);
            mainListView.Items.Add("Move Key Combination:  " + ConfigData.MoveKey);
            mainListView.Items.Add(String.Format("Static window size: ({0}, {1})", ConfigData.StaticWidth, ConfigData.StaticHeight));

            mainListView.SelectedItem = 0;
        }

        private void UI_PopulateProcessBox()
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                Dispatcher.Invoke((Action)(() => processComboBox.Items.Add(process.ProcessName)));
            }
        }

        private void UI_RefreshSettings()
        {
            UI_PopulateSettings();
            StartWatchBtn.IsEnabled = false;
        }

        public void StartWatch()
        {
            WindowsReStyle();

            if (!UiFoundProcess)
            {
                MessageBox.Show("Could not find the process " + ConfigData.ProcessName,
                    "Windows Frameless Terminal", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }

            WindowPointer = CoreWindow.FindWindow(ConfigData.ProcessName, null);

            CoreWindow.GetWindowRect(WindowPointer, out CurrentWindowRectangle);

            CoreWindow.SetWindowPos(WindowPointer, 0, CurrentWindowRectangle.X, CurrentWindowRectangle.Y,
                CurrentWindowRectangle.Width, CurrentWindowRectangle.Height - 1, 0);

            if (windowManagerThread.IsAlive) windowManagerThread.Abort();

            if (WindowPointer != IntPtr.Zero) windowManagerThread.Start();

            StartWatchBtn.IsEnabled = false;

            uint processId;
            CoreWindow.GetWindowThreadProcessId(WindowPointer, out processId);
            InfoLbl.Content = "Info: PID: " + Convert.ToString(processId);

            SystemSounds.Beep.Play();
        }

        public static void WindowsReStyle()
        {
            Process[] Procs = Process.GetProcesses();
            foreach (Process proc in Procs)
            {
                if (proc.ProcessName.StartsWith(ConfigData.ProcessName))
                {
                    IntPtr pFoundWindow = proc.MainWindowHandle;
                    int style = CoreWindow.GetWindowLong(pFoundWindow, CoreWindow.GWL_STYLE);
                    CoreWindow.SetWindowLong(pFoundWindow, CoreWindow.GWL_STYLE, (style & ~CoreWindow.WS_CAPTION));

                    UiFoundProcess = true;
                }
            }
        }
        static void WindowManager()
        {
            System.Drawing.Rectangle originalWindowRect;
            CoreWindow.GetWindowRect(WindowPointer, out originalWindowRect);

            Console.WriteLine("Original Window: " +  originalWindowRect);

            while (true)
            {
                IntPtr currentWindow = CoreWindow.GetForegroundWindow();

                if (IsDraggingWindow && currentWindow == WindowPointer)
                {
                    System.Drawing.Point hostCursorPosition = CoreMouse.GetCursorPosition();
                    
                    CoreWindow.MoveWindow(WindowPointer, hostCursorPosition.X, hostCursorPosition.Y,
                      ConfigData.StaticWidth, ConfigData.StaticHeight, true);

                    Console.WriteLine(hostCursorPosition);
                }
                else if(IsDraggingWindow == false && currentWindow == WindowPointer)
                {
                    CoreWindow.GetWindowRect(WindowPointer, out originalWindowRect);
                    Console.WriteLine("REC: " + originalWindowRect);
                }
            }
        }

        public MainWindow()
        {
            keyboardListener.KeyDown += new RawKeyEventHandler(KListener_KeyDown);
            Config.ParseConfig();

            InitializeComponent();

            var populateProcessList = new Thread(UI_PopulateProcessBox);
            populateProcessList.Start();

            UI_PopulateSettings();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowsReStyle();
            StartWatch();

            makeProcess.IsEnabled = false;
            processComboBox.IsEnabled = false;
        }

        void KListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            if(args.Key.ToString() == ConfigData.MoveKey)
            {
                IsDraggingWindow = !IsDraggingWindow;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            keyboardListener.Dispose();
        }

        private void ReloadSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            Config.ParseConfig();
            UI_RefreshSettings();

            StartWatchBtn.IsEnabled = true;
            SystemSounds.Beep.Play();
        }

        private void makeProcess_Click(object sender, RoutedEventArgs e)
        {
            if (processComboBox.SelectedItem.ToString().Equals(string.Empty))
                return;

            ConfigData.ProcessName = processComboBox.SelectedItem.ToString();
            mainListView.Items.Add("Temporary process: " + ConfigData.ProcessName);
            SystemSounds.Beep.Play();
        }
    }
}
