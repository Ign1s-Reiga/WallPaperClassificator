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
using System.Runtime.InteropServices;
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

		private void StartClassificate_Click(object sender, RoutedEventArgs args)
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
				ResultData<string> saveDirResult = IOUtils.CreateDirectory(this.saveImageDirPath, false);
				if (saveDirResult.Status == ResultStatus.Error)
				{
					PopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, saveDirResult.Value);
					return;
				}
				ResultData<string> tmpDirResult = IOUtils.CreateDirectory(this.tmpDirPath);
				if (tmpDirResult.Status == ResultStatus.Error)
				{
					PopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, tmpDirResult.Value);
					return;
				}
				List<ClassificateListItemData> imageList = CopyToTmpDir(new ConcurrentBag<ClassificateListItemData>(Files))
					.OrderBy(item => item.FileDescription.FileName, new StringComparer())
					.ToList();
				ClassificateWindow clsfWindow = new ClassificateWindow(imageList);
				clsfWindow.Closed += delegate {
					MainWindow.Instance.AppWindow.Show();
					imageList.ForEach(Files.Add);
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

		private async void SaveImages_Click(object sender, RoutedEventArgs e)
		{
			ContentDialogResult result = ContentDialogResult.Primary;
			if (Files.Where(file => file.State == ClassificateState.None).Any())
			{
				ContentDialog confirmDialog = new ContentDialog
				{
					Title = "Classification is not finished yet",
					Content = "There are unclassified image(s) in the list. Are you okay to continue?",
					CloseButtonText = "No",
					PrimaryButtonText = "Yes",
					DefaultButton = ContentDialogButton.Close,
					XamlRoot = XamlRoot
				};

				result = await confirmDialog.ShowAsync();
			}

			if (result == ContentDialogResult.Primary)
			{
				IEnumerable<string> listToSave = Files.Where(file => file.State == ClassificateState.Save)
					.Select(file => file.FileDescription.FullPath);
				listToSave.ToList().ForEach(file =>
				{
					string destPath = Path.Combine(this.saveImageDirPath, Path.GetFileName(file));
					try
					{
						File.Copy(file, destPath, true);
					}
					catch (Exception e)
					{
						Console.WriteLine($"Failed to copy: From: {file}, Dest: {destPath}, Message: {e.Message}");
					}
				});
				Files.Clear();
			}
			try
			{
				Directory.Delete(this.tmpDirPath, true);
			}
			catch (Exception)
			{
				PopupInfoBar.AddInfoBar(InfoBarSeverity.Error, "Failed to delete the tmp directory. Please delete it manually.");
			}
			finally
			{
				SaveImagesButton.IsEnabled = false;
				if (result == ContentDialogResult.Primary)
					PopupInfoBar.AddInfoBar(InfoBarSeverity.Success, "The images have been classificated successfully.");
				else
					PopupInfoBar.AddInfoBar(InfoBarSeverity.Informational, "Image saving process has been canceled.");
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

		private ConcurrentBag<ClassificateListItemData> CopyToTmpDir(ConcurrentBag<ClassificateListItemData> items)
		{
			DirectoryInfo unclsfDirInfo = new DirectoryInfo(UnclassifiedImageDirPath.Text);
			
			ConcurrentBag<FileInfo> images = new ConcurrentBag<FileInfo>();
			FilterByIsImage(unclsfDirInfo.EnumerateFiles(), images);

			ConcurrentBag<ClassificateListItemData> updatedItems = new ConcurrentBag<ClassificateListItemData>();
			int maxParallelism = (int)App.Settings.NumThreadsConvImages;
			Parallel.ForEach(images, new ParallelOptions { MaxDegreeOfParallelism = maxParallelism }, info =>
			{
				string destPath = Path.Combine(this.tmpDirPath, info.Name);
				info.CopyTo(destPath, true);

				using Image image = Image.Load(destPath);
				if (Path.GetExtension(info.Name).ToLower() == ".webp")
				{
					string newPath = Path.ChangeExtension(destPath, ".png");
					image.SaveAsPng(newPath);
					File.Delete(destPath);
					destPath = newPath;
				}

				ClassificateListItemData? existingItem = items.FirstOrDefault(item => item.FileDescription.FileName == Path.GetFileName(destPath));
				try
				{
					IOUtils.ComputeFileHash(destPath, out byte[] fileHash);

					if (existingItem == null || !existingItem.FileDescription.HashArray.SequenceEqual(fileHash))
					{
						updatedItems.Add(new ClassificateListItemData
						{
							FileDescription = new FileDescription
							{
								FileName = Path.GetFileName(destPath),
								FullPath = destPath,
								HashArray = fileHash
							},
							State = ClassificateState.None,
							Symbol = "\uF141" // hyphen symbol
						});
					}
					else
					{
						updatedItems.Add(existingItem);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine($"An exception while processing file {info.Name}: {e.Message}");
				}
			});

			return updatedItems;
		}

		private void FilterByIsImage(IEnumerable<FileInfo> fileList, ConcurrentBag<FileInfo> imageList)
		{
			string[] MIMETypes = ["image/jpeg", "image/png", "image/gif", "image/bmp", "image/tiff", "image/webp"];
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
					Console.WriteLine($"Failed to detect image format: {e.Message}");
				}

				if (isImage)
				{
					imageList.Add(info);
				}
			});
		}
	}

	public class StringComparer : IComparer<string>
	{
		[DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		static extern int StrCmpLogicalW(string x, string y);

		public int Compare(string? x, string? y) => x != null && y != null
			? StrCmpLogicalW(x, y)
			: 0;
	}
}
