using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;

namespace WallPaperClassificator
{
	public sealed partial class MainWindow : Window
	{
        public MainWindow()
		{
			this.InitializeComponent();
			this.Title = "WallPaper Classificator";
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
			if (args.IsSettingsInvoked)
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
				navView.Header = item.Content.ToString();
				ContentFrame.Navigate(pageType, null, info);
			}
		}
	}
}
