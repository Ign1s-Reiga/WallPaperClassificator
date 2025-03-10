using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

namespace WallPaperClassificator.Component
{
	public sealed partial class PopupInfoBar : UserControl
	{
		public PopupInfoBar()
		{
			this.InitializeComponent();
		}

		public void AddInfoBar(InfoBarSeverity severity, string message)
		{
			if (InfoBarPanel.Children.Count > 0 && InfoBarPanel.Children.First() is InfoBar prevInfoBar)
			{
				prevInfoBar.IsOpen = false;
			}
			InfoBar infoBar = new InfoBar
			{
				IsOpen = true,
				Severity = severity,
				Title = severity.ToString(),
				Message = message
			};
			infoBar.Closed += InfoBar_Closed;
			infoBar.Loaded += InfoBar_Loaded;
			InfoBarPanel.Children.Add(infoBar);
		}

		private void InfoBar_Loaded(object sender, RoutedEventArgs e)
		{
			InfoBar infoBar = (InfoBar)sender;
			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(5);
			timer.Tick += (object? sender, object e) =>
			{
				infoBar.IsOpen = false;
				timer.Stop();
			};
			timer.Start();
		}

		private void InfoBar_Closed(object sender, InfoBarClosedEventArgs e)
		{
			InfoBar infoBar = (InfoBar)sender;
			InfoBarPanel.Children.Remove(infoBar);
		}
	}
}
