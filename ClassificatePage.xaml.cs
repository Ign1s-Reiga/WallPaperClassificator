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
using System.Diagnostics;

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
			// This condition will delete when ClassificateWindow is completely implemented.
			if (SaveImageDirPath.Text.Length == 0)
			{
				PopupInfoBar.AddInfoBar(InfoBarSeverity.Warning, "Please select the folder that will used to save the images.");
				return;
			}

			if (Directory.Exists(UnclassifiedImageDirPath.Text) && Directory.Exists(SaveImageDirPath.Text))
			{
				List<ClassificateListItemData> classifiedImageList = new List<ClassificateListItemData>();
				ClassificateWindow clsfWindow = new ClassificateWindow(UnclassifiedImageDirPath.Text, SaveImageDirPath.Text, classifiedImageList);
				clsfWindow.Closed += delegate {
					MainWindow.Instance.AppWindow.Show();
					classifiedImageList.ForEach((item) =>
					{
						Debug.WriteLine($"Classified Image: {item.FileName}");
					});
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
	}
}
