// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Migrate.Explore.Authentication;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Threading.Tasks;
using AzureMigrateExplore.Authentication;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AzureMigrateExplore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Welcome : Page
    {
        public Welcome()
        {
            this.InitializeComponent();
            VersionLabel.Text = GetVersion();
        }

        public event EventHandler LoginButtonClicked;

        private string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly()
                                  .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                  ?.InformationalVersion;
            return version ?? "v1.0.0";
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            //Initialize the Project Details constructor
            LoginButtonClicked?.Invoke(this, EventArgs.Empty);

            // Initialize the PeriodicTaskService when the app starts up
            DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            PeriodicSessionRefresh.Initialize(dispatcherQueue);
        }
    }
}
