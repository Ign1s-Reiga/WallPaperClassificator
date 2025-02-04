using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WallPaperClassificator.Util
{
	public class WallPaperHelper
	{
		[DllImport("user32.dll")]
		private static extern bool SystemParametersInfo(int uiAction, int uiParam, string pvParam, int fWinIni);

		// Return WallPaper path before classificating
		public static string? SetWallPaper(string path)
		{
			const int SPI_SETDESKWALLPAPER = 0x0014;
			const int SPIF_UPDATEINIFILE = 0x01;
			const int SPIF_SENDCHANGE = 0x02;
			SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

			RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop");
			string? wallPaperPath = key != null ? (string?)key.GetValue("WallPaper") : null;

			return wallPaperPath;
		}
	}
}
