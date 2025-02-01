using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace WallPaperClassificator.Component
{
	public sealed partial class PopupInfoBar : UserControl
	{
		public PopupInfoBar()
		{
			this.InitializeComponent();
			Canvas.SetLeft(InfoBarPanel, InfoBarCanvas.ActualWidth);
		}

		public void AddInfoBar(InfoBarSeverity severity, string message)
		{
			if (InfoBarPanel.Children.Count > 0 && InfoBarPanel.Children.First() is InfoBar prevInfoBar)
			{
				prevInfoBar.IsOpen = false;
			}
			InfoBar infoBar = new InfoBar();
			infoBar.Closed += InfoBar_Closed;
			infoBar.Loaded += InfoBar_Loaded;
			infoBar.IsOpen = true;
			infoBar.Severity = severity;
			infoBar.Title = severity.ToString();
			infoBar.Message = message;
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
