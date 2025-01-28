using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WallPaperClassificator
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public List<string> Files { get; set; }
        public MainWindow()
        {
            Files = [];
            this.InitializeComponent();
            this.Title = "WallPape Classificator";
		}
        
        private void StartClassificate_Click(object sender, RoutedEventArgs args)
        {
            Console.WriteLine(WallPaperDirPath.Text);
            Console.WriteLine("TEST");

			if (WallPaperDirPath.Text is null)
				return;
			string path = WallPaperDirPath.Text;

			try
            {
				var wallpaperDir = Directory.EnumerateFiles(path);
				Files = wallpaperDir.ToList();
			} catch(Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
		}

        private void CurrentWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
		{
			BackgroundPanel.Width = e.Size.Width;
		}
	}
}
