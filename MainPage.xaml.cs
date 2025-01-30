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
				return;
			string path = NonClassifiedImageDirPath.Text;

			if (Directory.Exists(path))
			{
				// Minimize window and create subwindow.
			}
		}

		private async void SelectFolder_Click(object sender, RoutedEventArgs args)
		{
			FolderPicker picker = new FolderPicker();
			picker.FileTypeFilter.Add("*");

			nint hWnd = WindowNative.GetWindowHandle(this);
			InitializeWithWindow.Initialize(picker, hWnd);

			StorageFolder? folder = await picker.PickSingleFolderAsync();
			if (folder != null)
			{
				TextBox textBox = (TextBox)sender;
				if (textBox.Name == "NonClassificatedWallPaperDirPath")
					NonClassifiedImageDirPath.Text = folder.Path;
				else if (textBox.Name == "SaveImageDirPath")
					SaveImageDirPath.Text = folder.Path;
				else
					return;
			}
		}
	}
}
