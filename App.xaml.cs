using Microsoft.UI.Xaml;
using WallPaperClassificator.Util;

namespace WallPaperClassificator
{
	public partial class App : Application
    {
        public static ISettings Settings { get; set; } = SettingsHelper.ReadAppSettings();

        private Window? window;

		public App()
        {
            this.RequestedTheme = SettingsHelper.GetApplicationThemeFromInt(Settings.AppTheme);
			this.InitializeComponent();
        }
    
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            window = new MainWindow();
            window.Activate();
		}
    }
}
