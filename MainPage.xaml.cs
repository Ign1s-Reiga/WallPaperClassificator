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
			if (NonClassifiedImageDirPath.Text.Length == 0)
			{
				ClassificatePopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, "Please select the folder where the images are stored.");
				return;
			}
			string path = NonClassifiedImageDirPath.Text;

			if (Directory.Exists(path))
			{
				ClassificateWindow clsfWindow = new ClassificateWindow();
				clsfWindow.Activate();
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

			MainWindow? instance = MainWindow.Instance;
			if (instance == null) {
				ClassificatePopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, "Failed to Open Folder Picker.");
				return;
			}
			nint hWnd = WindowNative.GetWindowHandle(instance);
			InitializeWithWindow.Initialize(picker, hWnd);

			StorageFolder? folder = await picker.PickSingleFolderAsync();
			if (folder != null)
			{

				Button button = (Button)sender;
				if (button.Name == "NonClassifiedImageDirButton")
					NonClassifiedImageDirPath.Text = folder.Path;
				else if (button.Name == "SaveImageDirButton")
					SaveImageDirPath.Text = folder.Path;
				else
					return;
			}
		}
	}
}
