using Microsoft.UI.Xaml;
using System.Collections.Generic;
using WallPaperClassificator.Util;

namespace WallPaperClassificator
{
    public partial class App : Application
    {
        public static Dictionary<string, object> Settings { get; private set; } = new Dictionary<string, object>();

		public App()
        {
			Settings = SettingsHelper.ReadAppSettings();
			this.RequestedTheme = SettingsHelper.GetApplicationThemeFromInt((int)Settings["appTheme"]);

			this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
			window = new MainWindow();
			window.Activate();
            window.Closed += delegate
            {
                SettingsHelper.WriteAppSettings(Settings);
			};
		}

		private Window? window;
    }
}
