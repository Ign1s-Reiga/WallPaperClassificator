using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace WallPaperClassificator
{
	public sealed partial class MainWindow : Window
    {
        public List<string> Files { get; set; }
        public MainWindow()
        {
            Files = []; // The Array of Classificated Files' Path.
            this.InitializeComponent();
            this.Title = "WallPaper Classificator";
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

        private void CurrentWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
		{
			BackgroundPanel.Width = e.Size.Width;
			BackgroundPanel.Height = e.Size.Height;
		}
	}
}
