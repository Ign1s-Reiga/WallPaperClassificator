using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.Storage;

namespace WallPaperClassificator.Util
{
	public class SettingsHelper
	{
		private static readonly Dictionary<string, string> defaultSetting = new Dictionary<string, string>() {
			{  "fallbackWallPaperPath", @"C:\Windows\Web\Wallpaper\Windows\img0.jpg" }
		};
		private static readonly uint settingsVersion = 1;

		// Read settings from app settings
		public static Dictionary<string, object> ReadAppSettings()
		{
			ApplicationData current = ApplicationData.Current;
			if (current.LocalSettings.Values.Count() == 0)
				InitializeAppSettings(current, false);
			else if (current.Version < settingsVersion)
				InitializeAppSettings(current, true);

			return current.LocalSettings.Values.ToDictionary();
		}

		// Write settings to app settings
		public static void WriteAppSettings(Dictionary<string, object> settings)
		{
			ApplicationData current = ApplicationData.Current;
			foreach (KeyValuePair<string, object> setting in settings)
				current.LocalSettings.Values[setting.Key] = setting.Value;
		}

		// Initialize app settings
		public static void InitializeAppSettings(ApplicationData current, bool upgrade)
		{
			ApplicationDataContainer previousSettings = current.LocalSettings;
			Task.Run(async () =>
			{
				await current.SetVersionAsync(settingsVersion, handler =>
				{
					Debug.WriteLine((upgrade ? "Upgrading" : "Initializing") + $" Settings: {handler.DesiredVersion}");
					foreach (KeyValuePair<string, string> settings in defaultSetting)
					{
						if (upgrade && previousSettings.Values.TryGetValue(settings.Key, out object? value))
							current.LocalSettings.Values[settings.Key] = value;
						else
							current.LocalSettings.Values[settings.Key] = settings.Value;
					}
				});
			});
		}
	}
}
