// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI; // Add this using directive

namespace System.Windows.Forms
{
    static class ExtendTabButtonEnableDisable
    {
        public static void EnableTabButton(this Button button)
        {
            button.Enabled = true;
            //button.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 255, 255, 255)); // Use Background instead of BackgroundColor
        }

        public static void DisableTabButton(this Button button)
        {
            button.Enabled = false;
            //button.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 170, 170, 170)); // Use Background instead of BackgroundColor
        }
    }
}
