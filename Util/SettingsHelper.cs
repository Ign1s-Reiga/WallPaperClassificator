using Config.Net;
using Microsoft.UI.Xaml;
using System.ComponentModel;

namespace WallPaperClassificator.Util
{
	public class SettingsHelper
	{
		private static readonly int settingsVersion = 1;
		private static readonly string settingsFileName = "settings.json";

		// Read settings from app settings
		public static ISettings ReadAppSettings()
		{
			ISettings settings =  new ConfigurationBuilder<ISettings>()
				.UseJsonFile(settingsFileName)
				.Build();
			if (settings.Version < settingsVersion) // Upgrade phase
			{
				settings.Version = settingsVersion;
			}
			return settings;
		}

		public static ApplicationTheme GetApplicationThemeFromInt(int theme)
		{
			return theme switch
			{
				0 => ApplicationTheme.Light,
				1 => ApplicationTheme.Dark,
				_ => ApplicationTheme.Light
			};
		}
	}

	public interface ISettings
	{
		[DefaultValue(1)]
		public int Version { get; set; }


		public int AppTheme { get; set; }

		[DefaultValue(@"C:\Windows\Web\Wallpaper\Windows\img0.jpg")]
		public string FallbackWallPaperPath { get; set; }
	}
}
