using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;

namespace WallPaperClassificator
{
	public sealed partial class ClassificatePage : Page
	{
		public List<string> Files { get; set; }

		public ClassificatePage()
		{
			Files = []; // The Array of Classificated Files' Path.
			this.InitializeComponent();
		}

		private void StartClassificate_Click(object sender, RoutedEventArgs args)
		{
			if (UnclassifiedImageDirPath.Text.Length == 0)
			{
				PopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, "Please select the folder where the images are stored.");
				return;
			}
			string path = UnclassifiedImageDirPath.Text;

			if (Directory.Exists(path))
			{
				ClassificateWindow clsfWindow = new ClassificateWindow();
				clsfWindow.Closed += delegate {
					MainWindow.Instance?.AppWindow.Show();
				};
				clsfWindow.Activate();
				MainWindow.Instance?.AppWindow.Hide();
			}
			else
			{
				PopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, "The folder does not exist.");
			}
		}

		private async void SelectFolder_Click(object sender, RoutedEventArgs args)
		{
			FolderPicker picker = new FolderPicker();
			picker.FileTypeFilter.Add("*");

			nint hWnd = WindowNative.GetWindowHandle(MainWindow.Instance);
			InitializeWithWindow.Initialize(picker, hWnd);

			StorageFolder? folder = await picker.PickSingleFolderAsync();
			if (folder != null)
			{

				Button button = (Button)sender;
				if (button.Name == "NonClassifiedImageDirButton")
					UnclassifiedImageDirPath.Text = folder.Path;
				else if (button.Name == "SaveImageDirButton")
					SaveImageDirPath.Text = folder.Path;
				else
					return;
			}
		}
	}
}
