// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Graphics;
using WinRT.Interop;
using Microsoft.Windows.AppNotifications;
using System.Collections.Generic;
using static Azure.Core.HttpHeader;
using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.UI.Dispatching;
using AzureMigrateExplore.Authentication;

namespace AzureMigrateExplore
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public Frame rootFrame;
        public Window? m_window;
        private NotificationManager notificationManager;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            notificationManager = new NotificationManager();
            notificationManager.Init();
            this.RequestedTheme = ApplicationTheme.Light; // Set light theme as default
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            this.m_window = new Window();

            // Set the content to the root frame
            this.m_window.Content = rootFrame = new Frame();

            // Extend content into the title bar
            this.m_window.ExtendsContentIntoTitleBar = true;
            this.m_window.SetTitleBar(null);
            this.m_window.Title = "AzureMigrateExplore";

            // Get the window handle
            IntPtr hWnd = WindowNative.GetWindowHandle(this.m_window);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            //// Hide the default title bar
            appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            appWindow.TitleBar.ButtonBackgroundColor = Colors.Black;
            appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Black;
            appWindow.SetIcon("Assets/Azure-Migrate.ico");

            //// Customize the window presenter
            OverlappedPresenter presenter = appWindow.Presenter as OverlappedPresenter;
            if (presenter != null)
            {
                presenter.IsMaximizable = true;
                presenter.IsMinimizable = true;
                presenter.IsResizable = true;

                // Maximize the window
                presenter.Maximize();
                //presenter.SetBorderAndTitleBar(false, false);
            }

            // Activate the window
            this.m_window.Activate();

            // Navigate to the main window
            rootFrame.Navigate(typeof(MainWindow));

            // If you already have an exit event handler, add PeriodicTaskService.Shutdown() to it
            m_window.Closed += (sender, args) =>
            {
                // Shutdown the periodic task service when the app is closing
                PeriodicSessionRefresh.Shutdown();
            };
        }

        void OnProcessExit(object sender, EventArgs e)
        {
            notificationManager.Unregister();
        }
    }

    internal class NotificationManager
    {
        private bool m_isRegistered;

        public NotificationManager()
        {
            m_isRegistered = false;
        }

        ~NotificationManager()
        {
            Unregister();
        }

        public void Init()
        {
            // To ensure all Notification handling happens in this process instance, register for
            // NotificationInvoked before calling Register(). Without this a new process will
            // be launched to handle the notification.
            AppNotificationManager notificationManager = AppNotificationManager.Default;

            notificationManager.NotificationInvoked += OnNotificationInvoked;

            notificationManager.Register();
            m_isRegistered = true;
        }

        public void Unregister()
        {
            if (m_isRegistered)
            {
                AppNotificationManager.Default.Unregister();
                m_isRegistered = false;
            }
        }

        private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            // Handle notification activation
            // You can access the arguments and user input from the notification
            var arguments = args.Arguments;
            var userInput = args.UserInput;

            // Ensure the app window is activated
            if (App.Current is App app && app.m_window != null)
            {
                app.m_window.DispatcherQueue.TryEnqueue(() =>
                {
                    app.m_window.Activate();
                    app.rootFrame.Navigate(typeof(MainWindow), arguments);
                });
            }
        }
    }
}
