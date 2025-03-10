using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using WallPaperClassificator.Util;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace WallPaperClassificator
{
	public sealed partial class SettingsPage : Page
	{
		public SettingsPage()
		{
			this.InitializeComponent();
		}
		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			ApplicationTheme theme = SettingsHelper.GetApplicationThemeFromInt(AppThemeSettings.SelectedIndex);
			App.Settings["appTheme"] = AppThemeSettings.SelectedIndex;
		}

		private void SettingsPage_Loading(FrameworkElement sender, object args)
		{
			AppThemeSettings.SelectedIndex = (int)App.Settings["appTheme"];
		}
	}
}
