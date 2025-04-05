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

		private string saveImageDirPath = string.Empty;

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

			if (!CreateSaveImageDir())
			{
				PopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, "An error has occurred while creating save dir. please re-try");
				return;
			}

			if (Directory.Exists(UnclassifiedImageDirPath.Text))
			{
				List<ClassificateListItemData> classifiedImageList = new List<ClassificateListItemData>();
				ClassificateWindow clsfWindow = new ClassificateWindow(UnclassifiedImageDirPath.Text, classifiedImageList);
				clsfWindow.Closed += delegate {
					MainWindow.Instance.AppWindow.Show();
					classifiedImageList.ForEach(Files.Add);
					SaveImagesButton.IsEnabled = true;
				};
				clsfWindow.Activate();
				MainWindow.Instance.AppWindow.Hide();
			}
			else
			{
				PopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, "The folder does not exist.");
			}
		}

		private void SaveImages_Click(object sender, RoutedEventArgs e)
		{

			List<string> listToSave = Files.Where(file => file.State == ClassificateState.Save)
				.Select(file => file.FullPath)
				.ToList();

			if (IsPathDuplicated(listToSave))
			{
				PopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, "Some file(s) are couldn't be copied, because there are file(s) that have same name.");
				return;
			}
			listToSave.ForEach(file =>
			{
				string destPath = Path.Combine(this.saveImageDirPath, Path.GetFileName(file));
				try
				{
					File.Copy(file, destPath);
				}
				catch
				{
					// TODO: Log error
					return;
				}
			});
			PopupInfoBar.AddInfoBar(InfoBarSeverity.Success, "The images have been classificated successfully.");
			SaveImagesButton.IsEnabled = false;
			Files.Clear();
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

		private bool CreateSaveImageDir()
		{
			this.saveImageDirPath = SaveImageDirPath.Text != string.Empty
				? SaveImageDirPath.Text
				: Path.Combine(Directory.GetCurrentDirectory(), "Save");
			if (File.Exists(this.saveImageDirPath))
			{
				PopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, "The path is already allocated to a file. please delete it.");
				return false;
			}

			try
			{
				if (Directory.Exists(this.saveImageDirPath))
					return true;
				Directory.CreateDirectory(this.saveImageDirPath);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private bool IsPathDuplicated(List<string> files)
		{
			DirectoryInfo info = new DirectoryInfo(this.saveImageDirPath);
			return info.EnumerateFiles()
				.Where(file => files.Contains(Path.GetFileName(file.Name)))
				.Any();
		}
	}
}
