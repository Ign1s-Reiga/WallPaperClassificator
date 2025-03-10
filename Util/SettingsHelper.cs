using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace WallPaperClassificator.Util
{
	public class SettingsHelper
	{
		private static readonly Dictionary<string, object> defaultSettings = new Dictionary<string, object>() {
			{ "appTheme", 0 },
			{  "fallbackWallPaperPath", @"C:\Windows\Web\Wallpaper\Windows\img0.jpg" }
		};
		private static readonly uint settingsVersion = 1;

		// Read settings from app settings
		public static Dictionary<string, object> ReadAppSettings()
		{
			ApplicationData current = ApplicationData.Current;
			bool isFulfilled = Task.Run(async () =>
			{
				if (current.LocalSettings.Values.Count() == 0)
					await InitializeAppSettingsAsync(current, false);
				else if (current.Version < settingsVersion)
					await InitializeAppSettingsAsync(current, true);
			}).Wait(TimeSpan.FromMinutes(5));


			return isFulfilled ? current.LocalSettings.Values.ToDictionary() : defaultSettings;
		}

		// Write settings to app settings
		public static void WriteAppSettings(Dictionary<string, object> settings)
		{
			ApplicationData current = ApplicationData.Current;
			foreach (KeyValuePair<string, object> setting in settings)
				current.LocalSettings.Values[setting.Key] = setting.Value;
		}

		// Initialize app settings
		public static async Task InitializeAppSettingsAsync(ApplicationData current, bool upgrade)
		{
			ApplicationDataContainer previousSettings = current.LocalSettings;
			await current.SetVersionAsync(settingsVersion, handler =>
			{
				Debug.WriteLine((upgrade ? "Upgrading" : "Initializing") + $" Settings: {handler.DesiredVersion}");
				foreach (KeyValuePair<string, object> settings in defaultSettings)
				{
					if (upgrade && previousSettings.Values.TryGetValue(settings.Key, out object? value))
						current.LocalSettings.Values[settings.Key] = value;
					else
						current.LocalSettings.Values[settings.Key] = settings.Value;
				}
			});
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
}
