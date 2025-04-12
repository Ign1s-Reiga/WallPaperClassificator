using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WallPaperClassificator.Component
{
	public sealed partial class PopupInfoBar : UserControl
	{
		private readonly Queue<InfoBar> queue = new Queue<InfoBar>();

		public PopupInfoBar()
		{
			this.InitializeComponent();
		}

		public void AddInfoBar(InfoBarSeverity severity, string message)
		{
			InfoBar infoBar = new InfoBar
			{
				IsOpen = true,
				Severity = severity,
				Title = severity.ToString(),
				Message = message
			};
			infoBar.Closed += InfoBar_Closed;
			infoBar.Loaded += InfoBar_Loaded;
			if (InfoBarPanel.Children.Count > 0 && InfoBarPanel.Children.First() is InfoBar prevInfoBar)
			{
				queue.Enqueue(infoBar);
			}
			else
			{
				InfoBarPanel.Children.Add(infoBar);
			}
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

			if (queue.Count > 0)
			{
				InfoBar nextInfoBar = queue.Dequeue();
				InfoBarPanel.Children.Add(nextInfoBar);
			}
		}
	}
}
