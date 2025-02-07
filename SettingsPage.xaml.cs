using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
			ApplicationTheme theme = SettingsHelper.GetApplicationThemeFromInt(AppThemeSettings.SelectedIndex);
			// SettingsHelper.SetAppTheme(theme); need restart app.
			App.Settings["appTheme"] = AppThemeSettings.SelectedIndex;
		}

		private void SettingsPage_Loading(FrameworkElement sender, object args)
		{
			AppThemeSettings.SelectedIndex = (int)App.Settings["appTheme"];
		}
	}
}
