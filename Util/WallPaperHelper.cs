using System.Runtime.InteropServices;
using System.Text;

namespace WallPaperClassificator.Util
{
	public class WallPaperHelper
	{
		// Set String System parameter
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

		// Read String System parameter
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, StringBuilder pvParam, uint fWinIni);

		// Return WallPaper path before classificating
		// Heavy Operation, use async.
		public static void SetWallPaper(string path)
		{
			const uint SPI_SETDESKWALLPAPER = 0x0014;
			const uint SPIF_UPDATEINIFILE = 0x01;
			const uint SPIF_SENDCHANGE = 0x02;

			SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
		}

		public static string GetWallPaper()
		{
			const int SPI_GETDESKWALLPAPER = 0x0073;

			StringBuilder sb = new StringBuilder(512);
			bool success = SystemParametersInfo(SPI_GETDESKWALLPAPER, (uint)sb.Capacity, sb, 0);
			return success ? sb.ToString() : App.Settings.FallbackWallPaperPath;
		}
	}
}
