using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WallPaperClassificator.Util;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Image = SixLabors.ImageSharp.Image;

namespace WallPaperClassificator
{
	public sealed partial class ClassificatePage : Page
	{
		private ObservableCollection<ClassificateListItemData> Files = [];

		private string saveImageDirPath = string.Empty;
		private string tmpDirPath = string.Empty;

		public ClassificatePage()
		{
			this.InitializeComponent();
		}

		private async void StartClassificate_Click(object sender, RoutedEventArgs args)
		{
			if (UnclassifiedImageDirPath.Text.Length == 0)
			{
				PopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, "Please select the folder where the images are stored.");
				return;
			}

			if (Directory.Exists(UnclassifiedImageDirPath.Text))
			{

				this.saveImageDirPath = SaveImageDirPath.Text != string.Empty
					? SaveImageDirPath.Text
					: Path.Combine(Directory.GetCurrentDirectory(), "save");
				this.tmpDirPath = Path.Combine(UnclassifiedImageDirPath.Text, "tmp");
				ResultData<string> saveDirResult = CreateDirectory(this.saveImageDirPath, false);
				if (saveDirResult.status == ResultStatus.Error)
				{
					PopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, saveDirResult.value);
					return;
				}
				ResultData<string> tmpDirResult = CreateDirectory(this.tmpDirPath);
				if (tmpDirResult.status == ResultStatus.Error)
				{
					PopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, tmpDirResult.value);
					return;
				}
				await CopyToTmpDir();

				List<ClassificateListItemData> classifiedImageList = new List<ClassificateListItemData>();
				ClassificateWindow clsfWindow = new ClassificateWindow(this.tmpDirPath, classifiedImageList);
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
			Directory.Delete(this.tmpDirPath, true);
			SaveImagesButton.IsEnabled = false;
			Files.Clear();

			PopupInfoBar.AddInfoBar(InfoBarSeverity.Success, "The images have been classificated successfully.");
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

		private ResultData<string> CreateDirectory(string path, bool checkFileContains = true)
		{
			if (File.Exists(path))
			{
				return Result.Error<string>("The path is already allocated to a file. please delete it.");
			}

			if (Directory.Exists(path))
			{
				if (Directory.GetFiles(path).Length > 0 && checkFileContains)
				{
					return Result.Error<string>("The directory includes some file(s), please delete them.");
				}
				else
				{
					return Result.Ok<string>(path);
				}
			}
			else
			{
				try
				{
					Directory.CreateDirectory(path);
					return Result.Ok<string>(path);
				}
				catch (Exception e)
				{
					return Result.Error<string>(e.Message);
				}
			}
		}

		private async Task CopyToTmpDir()
		{
			DirectoryInfo unclsfDirInfo = new DirectoryInfo(UnclassifiedImageDirPath.Text);
			string[] acceptableMIMETypes = ["image/jpeg", "image/png", "image/gif", "image/bmp", "image/tiff", "image/webp"];
			ConcurrentBag<FileInfo> images = new ConcurrentBag<FileInfo>();
			await Task.Run(() => FilterByIsImage(unclsfDirInfo.EnumerateFiles(), images, acceptableMIMETypes));
			images.ToList().ForEach(image => Debug.WriteLine($"ImageList: {image.Name}"));
			await Task.Run(() =>
			{
				Parallel.ForEach(images, info =>
				{
					File.Copy(info.FullName, Path.Combine(this.tmpDirPath, info.Name), true);
				});
			});

			images.Clear();
			
			Debug.WriteLine(null);

			// Save webp images as png files (to be configurable)
			DirectoryInfo tmpDirInfo = new DirectoryInfo(this.tmpDirPath);
			await Task.Run(() => FilterByIsImage(tmpDirInfo.EnumerateFiles(), images, ["image/webp"]));
			images.ToList().ForEach(image => Debug.WriteLine($"ImageList: {image.Name}"));
			await Task.Run(() =>
			{
				Parallel.ForEach(images, info =>
				{
					using Image image = Image.Load(info.FullName);
					image.SaveAsPng(Path.Combine(this.tmpDirPath, Path.ChangeExtension(info.Name, ".png")));
					Debug.WriteLine($"Convert webp file to png: {info.Name}");
					info.Delete();
				});
			});
		}

		private void FilterByIsImage(IEnumerable<FileInfo> fileList, ConcurrentBag<FileInfo> imageList, string[] MIMETypes)
		{
			Parallel.ForEach(fileList, info =>
			{
				bool isImage = false;
				try
				{
					IImageFormat format = Image.DetectFormat(info.FullName);
					isImage = MIMETypes.Contains(format.DefaultMimeType);
				}
				catch (Exception e)
				{
					Debug.WriteLine(e.Message);
				}

				if (isImage)
				{
					imageList.Add(info);
				}
			});
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
