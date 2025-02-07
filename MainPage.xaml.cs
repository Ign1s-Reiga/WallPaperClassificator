using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;

namespace WallPaperClassificator
{
	public sealed partial class MainPage : Page
	{
		public List<string> Files { get; set; }

		public MainPage()
		{
			Files = []; // The Array of Classificated Files' Path.
			this.InitializeComponent();
		}

		private void StartClassificate_Click(object sender, RoutedEventArgs args)
		{
			if (UnclassifiedImageDirPath.Text.Length == 0)
			{
				ClassificatePopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, "Please select the folder where the images are stored.");
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
				ClassificatePopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, "The folder does not exist.");
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
