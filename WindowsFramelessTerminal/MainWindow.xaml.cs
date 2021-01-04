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
using System.Media;

namespace WindowsFramelessTerminal
{
    public partial class MainWindow : Window
    {
        public static IntPtr WindowPointer;
        public static bool IsDraggingWindow = false;
        public static System.Drawing.Rectangle CurrentWindowRectangle;

        public static bool UiFoundProcess;

        private readonly Thread windowManagerThread = new Thread(WindowManager);

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

            WindowPointer = WindowsAPI.FindWindow(ConfigData.ProcessName, null);

            WindowsAPI.GetWindowRect(WindowPointer, out CurrentWindowRectangle);

            WindowsAPI.SetWindowPos(WindowPointer, 0, CurrentWindowRectangle.X, CurrentWindowRectangle.Y,
                CurrentWindowRectangle.Width, CurrentWindowRectangle.Height - 1, 0);

            if (windowManagerThread.IsAlive) windowManagerThread.Abort();

            if (WindowPointer != IntPtr.Zero) windowManagerThread.Start();

            StartWatchBtn.IsEnabled = false;

            uint processId;
            WindowsAPI.GetWindowThreadProcessId(WindowPointer, out processId);
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
                    int style = WindowsAPI.GetWindowLong(pFoundWindow, WindowsAPI.GWL_STYLE);
                    WindowsAPI.SetWindowLong(pFoundWindow, WindowsAPI.GWL_STYLE, (style & ~WindowsAPI.WS_CAPTION));

                    UiFoundProcess = true;
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

                if (IsDraggingWindow && currentWindow == WindowPointer)
                {
                    System.Drawing.Point hostCursorPosition = WindowsAPI.GetCursorPosition();
                    
                    WindowsAPI.MoveWindow(WindowPointer, hostCursorPosition.X, hostCursorPosition.Y,
                      ConfigData.StaticWidth, ConfigData.StaticHeight, true);

                    Console.WriteLine(hostCursorPosition);
                }
                else if(IsDraggingWindow == false && currentWindow == WindowPointer)
                {
                    WindowsAPI.GetWindowRect(WindowPointer, out originalWindowRect);
                    Console.WriteLine("REC: " + originalWindowRect);
                }
            }
        }

        readonly KeyboardListener KListener = new KeyboardListener();
        public MainWindow()
        {
            KListener.KeyDown += new RawKeyEventHandler(KListener_KeyDown);
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
            KListener.Dispose();
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
