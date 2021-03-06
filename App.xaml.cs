﻿using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using XMLSigner.Library;

namespace XMLSigner
{
    public partial class App : Application
    {
        private static TaskbarIcon _taskBarIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            Process proc = Process.GetCurrentProcess();
            int count = Process.GetProcesses().Where(p =>
                p.ProcessName == proc.ProcessName).Count();

            if(count > 1)
            {
                MessageBox.Show("Already an instance is running...");
                Current.Shutdown();
            }
            AddTaskbarIcon();
            base.OnStartup(e);
            RegisterApplicationToRunOnStartup();
            //InitializeComponent();
        }

        internal static void ShowTaskbarNotificationAfterUpload(string message, BalloonIcon icon = BalloonIcon.Warning) //"Signed XML File Uploaded Successfully"
        {
            _taskBarIcon.ToolTipText = "BCC-CA XML Signing Client";
            _taskBarIcon.ShowBalloonTip("XML Signing Client", message, icon);
        }

        private void AddTaskbarIcon()
        {
            _taskBarIcon = (TaskbarIcon)FindResource("NotifyIcon");
            _taskBarIcon.Icon = Adapter.BytesToIcon(XMLSigner.Properties.Resources.Logo);
            _taskBarIcon.ToolTipText = "BCC-CA XML Signing Client";
            _taskBarIcon.Visibility = Visibility.Visible;
            //_taskBarIcon.ShowBalloonTip("XML Signing Client", "BCC-CA XML Signing Client is running in background", BalloonIcon.Info);
        }

        private void RegisterApplicationToRunOnStartup()
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                registryKey.SetValue(Application.ResourceAssembly.GetName().Name, Process.GetCurrentProcess().MainModule.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error - Application should be launched as Administrator, please run again.", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                Environment.Exit(0);
            }
        }

        void radioButton_CustomTSASelect(object sender, RoutedEventArgs e)
        {
            GetCustomTsaTextBox().IsEnabled = true;
        }

        private TextBox GetCustomTsaTextBox()
        {
            return LogicalTreeHelper.FindLogicalNode(_taskBarIcon.ContextMenu, "TSA") as TextBox;
        }

        void radioButton_CustomTSAUnSelect(object sender, RoutedEventArgs e)
        {
            GetCustomTsaTextBox().IsEnabled = false;
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            Current.Shutdown();
            Environment.Exit(0);
        }

        private void RestartApp_Click(object sender, RoutedEventArgs e)
        {
            HttpServer.RestartServer();
        }

        private void TryUpdateWithCustomTsa(object sender, RoutedEventArgs e)
        {
            //await Tsa.TrySettingNewTsaAsync(GetCustomTsaTextBox().Text);
            if(!Tsa.TrySettingNewTsa(GetCustomTsaTextBox().Text))
            {
                ShowTaskbarNotificationAfterUpload("TSA update Failed", BalloonIcon.Error);
            }
            else
            {
                ShowTaskbarNotificationAfterUpload("TSA updated successfully");
            }
        }
    }
}
