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
        public static bool isDraggingWindow = false;
        public static System.Drawing.Rectangle CurrentWindowRectangle;

        public static bool ui_FoundProcess;

        private Thread windowManagerThread = new Thread(WindowManager);

        private void UI_PopulateSettings()
        {
            mainListView.Items.Clear();
            mainListView.Items.Add("Process Name:  " + ConfigData.processName);
            mainListView.Items.Add("Move Key Combinaton:  " + ConfigData.moveKey);
            mainListView.Items.Add(String.Format("Static window size: ({0}, {1})", ConfigData.staticWidth, ConfigData.staticHeight));

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

            SystemSounds.Beep.Play();
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
                    System.Drawing.Point hostCursorPosition = WindowsAPI.GetCursorPosition();
                    
                    WindowsAPI.MoveWindow(WindowPointer, hostCursorPosition.X, hostCursorPosition.Y,
                      ConfigData.staticWidth, ConfigData.staticHeight, true);

                    Console.WriteLine(hostCursorPosition);
                }
                else if(isDraggingWindow == false && currentWindow == WindowPointer)
                {
                    WindowsAPI.GetWindowRect(WindowPointer, out originalWindowRect);
                    Console.WriteLine("REC: " + originalWindowRect);
                }
            }
        }

        KeyboardListener KListener = new KeyboardListener();
        public MainWindow()
        {
            KListener.KeyDown += new RawKeyEventHandler(KListener_KeyDown);
            Config.ParseConfig();

            InitializeComponent();

            Thread populteProcessList = new Thread(UI_PopulateProcessBox);
            populteProcessList.Start();

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

            StartWatchBtn.IsEnabled = true;
            SystemSounds.Beep.Play();
        }

        private void makeProcess_Click(object sender, RoutedEventArgs e)
        {
            if (processComboBox.SelectedItem.ToString().Equals(String.Empty))
                return;

            ConfigData.processName = processComboBox.SelectedItem.ToString();
            mainListView.Items.Add("Temproary process: " + ConfigData.processName);
            SystemSounds.Beep.Play();
        }
    }
}
