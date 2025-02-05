using Microsoft.UI.Xaml;
using System.Collections.Generic;
using WallPaperClassificator.Util;
using Windows.UI.ViewManagement;

namespace WallPaperClassificator
{
    public partial class App : Application
    {
        public static Dictionary<string, object> Settings { get; private set; } = new Dictionary<string, object>();
		public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Settings = SettingsHelper.ReadAppSettings();

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
