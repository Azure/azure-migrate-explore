// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI; // Add this using directive

namespace System.Windows.Forms
{
    static class ExtendActionButtonEnableDisable
    {
        public static void EnableActionButton(this Button button)
        {
            button.Enabled = true; // Use IsEnabled instead of Enabled
            //button.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 165, 206, 255)); // Use Background instead of BackgroundColor
        }

        public static void DisableActionButton(this Button button)
        {
            button.Enabled = false;
            //button.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 135, 135, 135)); // Use Background instead of BackgroundColor
        }
    }
}
