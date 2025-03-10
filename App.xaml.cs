using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using WallPaperClassificator.Util;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
    
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
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
