using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace WallPaperClassificator
{
	public sealed partial class MainWindow : Window
	{
		public static MainWindow Instance { get; private set; } = default!;
		public MainWindow()
		{
			Instance = this;
			this.InitializeComponent();
			this.ExtendsContentIntoTitleBar = true;
			this.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
		}

		private void CurrentWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
		{
			BackgroundPanel.Width = args.Size.Width;
			BackgroundPanel.Height = args.Size.Height;
		}

		private void MainNavView_Loaded(object sender, RoutedEventArgs e)
		{
			// TODO: Add HOME
			if (MainNavView.MenuItems.First() is NavigationViewItem item)
			{
				MainNavView.SelectedItem = item;
				MainNavView_Navigate(MainNavView, item, new EntranceNavigationTransitionInfo());
			}
		}

		private void MainNavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{
			if (args.IsSettingsInvoked && ContentFrame.CurrentSourcePageType != typeof(SettingsPage))
			{
				sender.Header = "Settings";
				ContentFrame.Navigate(typeof(SettingsPage), null, new EntranceNavigationTransitionInfo());
			}
			else if (sender.SelectedItem is NavigationViewItem item)
			{
				MainNavView_Navigate(sender, item, args.RecommendedNavigationTransitionInfo);
			}
		}

		private void MainNavView_Navigate(NavigationView navView, NavigationViewItem item, NavigationTransitionInfo info)
		{
			string pageName = ("WallPaperClassificator." + (string)item.Tag);
			Type? pageType = Type.GetType(pageName);
			if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
			{
				navView.Header = item.Content;
				ContentFrame.Navigate(pageType, null, info);
			}
		}
	}
}
