using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Linq;

namespace WallPaperClassificator
{
	public sealed partial class MainWindow : Window
	{
		public static MainWindow? Instance { get; private set; }
		public MainWindow()
		{
			Instance = this;
			this.InitializeComponent();
			this.ExtendsContentIntoTitleBar = true;
			this.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
		}

        private void CurrentWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
		{
			BackgroundPanel.Width = e.Size.Width;
			BackgroundPanel.Height = e.Size.Height;
		}

		private void MainNavView_Loaded(object sender, RoutedEventArgs e)
		{
			// TODO: Add Home
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
				ContentFrame.Navigate(typeof(SettingsPage));
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
