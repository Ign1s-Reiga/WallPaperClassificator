using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace WallPaperClassificator
{
	public sealed partial class ClassificatePage : Page
	{
		private ObservableCollection<ClassificateListItemData> Files = [];

		public ClassificatePage()
		{
			this.InitializeComponent();
		}

		private void StartClassificate_Click(object sender, RoutedEventArgs args)
		{
			if (UnclassifiedImageDirPath.Text.Length == 0)
			{
				PopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, "Please select the folder where the images are stored.");
				return;
			}
			// This condition will delete when ClassificateWindow is completely implemented.
			if (SaveImageDirPath.Text.Length == 0)
			{
				PopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, "Please select the folder that will used to save the images.");
				return;
			}

			if (Directory.Exists(UnclassifiedImageDirPath.Text) && Directory.Exists(SaveImageDirPath.Text))
			{
				List<ClassificateListItemData> classifiedImageList = new List<ClassificateListItemData>();
				ClassificateWindow clsfWindow = new ClassificateWindow(UnclassifiedImageDirPath.Text, classifiedImageList);
				clsfWindow.Closed += delegate {
					MainWindow.Instance.AppWindow.Show();
					classifiedImageList.ForEach(Files.Add);
				};
				clsfWindow.Activate();
				MainWindow.Instance.AppWindow.Hide();
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
				if (button.Name == "UnclassifiedImageDirButton")
					UnclassifiedImageDirPath.Text = folder.Path;
				else if (button.Name == "SaveImageDirButton")
					SaveImageDirPath.Text = folder.Path;
				else
					return;
			}
		}

		private async Task<bool> IsImageFileAsync(string path)
		{
			try
			{
				StorageFile file = await StorageFile.GetFileFromPathAsync(path);
				string contentType = file.ContentType;
				string[] acceptableImgMimeType = ["image/jpeg", "image/png", "image/gif", "image/bmp", "image/tiff"];

				return acceptableImgMimeType.Contains(contentType);
			}
			catch
			{
				return false;
			}
		}

		private bool CreateSaveImageDir(string path)
		{
			try
			{
				if (Directory.Exists(path))
					return false;

				Directory.CreateDirectory(path);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
