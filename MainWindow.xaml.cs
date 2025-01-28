using Microsoft.UI.Xaml;
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
            this.Title = "WallPape Classificator";
		}
        
        private void StartClassificate_Click(object sender, RoutedEventArgs args)
        {
			if (WallPaperDirPath.Text.Length == 0)
				return;
			string path = WallPaperDirPath.Text;

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
				WallPaperDirPath.Text = folder.Path;
			}
        }

        private void CurrentWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
		{
			BackgroundPanel.Width = e.Size.Width;
			BackgroundPanel.Height = e.Size.Height;
		}
	}
}
