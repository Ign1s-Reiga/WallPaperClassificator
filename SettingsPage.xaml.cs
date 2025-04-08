using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using WallPaperClassificator.Util;

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
			App.Settings.AppTheme = AppTheme.SelectedIndex;
			App.Settings.FallbackWallPaperPath = FallbackWallPaperPath.Text;
			App.Settings.NumThreadsConvImages = NumThreadsConvImages.Value;
		}

		private void SettingsPage_Loading(FrameworkElement sender, object args)
		{
			AppTheme.SelectedIndex = App.Settings.AppTheme;
			FallbackWallPaperPath.Text = App.Settings.FallbackWallPaperPath;
			NumThreadsConvImages.Value = App.Settings.NumThreadsConvImages;
		}
	}
}
