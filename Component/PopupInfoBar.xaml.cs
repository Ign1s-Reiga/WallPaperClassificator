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

namespace WallPaperClassificator.Component
{
	public sealed partial class PopupInfoBar : UserControl
	{
		public PopupInfoBar()
		{
			this.InitializeComponent();
			InfoBar.Severity = Severity;
			InfoBar.Message = Message;
		}

		private void InfoBar_Closed(object sender, InfoBarClosedEventArgs e)
		{
			DispatcherTimer timer = new DispatcherTimer();
			timer.Tick += (s, e) =>
			{
				if (s is null) return;
				if (((DispatcherTimer)s).IsEnabled || !InfoBar.IsOpen)
				{
					InfoBar.IsOpen = true;
					((DispatcherTimer)s).Stop();
				}
			};
			timer.Interval = TimeSpan.FromSeconds(5);
			timer.Start();
		}

		public static readonly DependencyProperty SeverityProperty = DependencyProperty.Register(
			"Severity",
			typeof(InfoBarSeverity),
			typeof(PopupInfoBar),
			new PropertyMetadata(InfoBarSeverity.Informational)
		);

		public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
			"Message",
			typeof(string),
			typeof(PopupInfoBar),
			new PropertyMetadata("")
		);

		public InfoBarSeverity Severity
		{
			get => (InfoBarSeverity)GetValue(SeverityProperty);
			set => SetValue(SeverityProperty, value);
		}

		public string Message
		{
			get => (string)GetValue(MessageProperty);
			set => SetValue(MessageProperty, value);
		}
	}
}
