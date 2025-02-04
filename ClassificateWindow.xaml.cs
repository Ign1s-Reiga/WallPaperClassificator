using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using WinRT.Interop;

namespace WallPaperClassificator
{
	public sealed partial class ClassificateWindow : Window
	{
		[DllImport("user32.dll")]
		private static extern int GetDpiForWindow(nint hWnd);

		[DllImport("user32.dll")]
		private static extern bool GetCursorPos(out PointInt32 lpPoint);

		public List<String> StringList = ["1", "545", "414", "222"];

		private int posWinX = 0, posWinY = 0, posPressedX = 0, posPressedY = 0;
		private bool pointerMoving = false;

		public ClassificateWindow()
		{
			this.InitializeComponent();
			this.ExtendsContentIntoTitleBar = true;

			OverlappedPresenter presenter = (OverlappedPresenter)this.AppWindow.Presenter;
			presenter.IsMaximizable = false;
			presenter.IsMinimizable = false;
			presenter.IsResizable = false;
			presenter.SetBorderAndTitleBar(true, false);

			ClassificateWindow_Resize(800, 96);
		}

		private void ClassificateWindowExpander_SizeChanged(object sender, SizeChangedEventArgs args)
		{
			const int EXPANDER_DEFAULT_HEIGHT = 48;
			Expander expander = (Expander)sender;

			double desiredHeight = expander.ActualHeight != EXPANDER_DEFAULT_HEIGHT
				? 96 + expander.ActualHeight - EXPANDER_DEFAULT_HEIGHT
				: 96;
			ClassificateWindow_Resize(800, desiredHeight);
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void ClassificateWindowBackPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
		{
			((UIElement)sender).ReleasePointerCapture(e.Pointer);
			pointerMoving = false;
		}

		private void ClassificateWindowBackPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var properties = e.GetCurrentPoint((UIElement)sender).Properties;
			if (properties.IsLeftButtonPressed)
			{
				((UIElement)sender).CapturePointer(e.Pointer);
				posWinX = this.AppWindow.Position.X;
				posWinY = this.AppWindow.Position.Y;
				PointInt32 pt;
				GetCursorPos(out pt);
				posPressedX = pt.X;
				posPressedY = pt.Y;
				pointerMoving = true;
			}
		}

		private void ClassificateWindowBackPanel_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			PointerPointProperties props = e.GetCurrentPoint((UIElement)sender).Properties;
			if (props.IsLeftButtonPressed)
			{
				((UIElement)sender).CapturePointer(e.Pointer);
				PointInt32 pt;
				GetCursorPos(out pt);

				if (pointerMoving)
				{
					this.AppWindow.Move(new PointInt32(posWinX + pt.X - posPressedX, posWinY + pt.Y - posPressedY));
				}
			}
		}

		private void ClassificateWindow_Resize(double width, double height)
		{
			int dpi = GetDpiForWindow(WindowNative.GetWindowHandle(this));
			double dpiScale = dpi / 96D;

			this.AppWindow.Resize(new SizeInt32(
				(int)(width * dpiScale),
				(int)(height * dpiScale)
			));
		}
	}
}
