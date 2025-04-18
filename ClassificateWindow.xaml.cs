using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WallPaperClassificator.Util;
using Windows.Foundation;
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

		private ObservableCollection<ClassificateListItemData> FileList;

		private readonly string defaultWallPaperPath;
		private readonly List<ClassificateListItemData> classifiedImageList;

		private int posWinX = 0, posWinY = 0, posPressedX = 0, posPressedY = 0;
		private bool pointerMoving = false;
		private int selectedIndexCache = -1; // This value will contains the index of the item that was selected before the refresh request.

		public ClassificateWindow(List<ClassificateListItemData> classifiedImageList)
		{
			this.defaultWallPaperPath = WallPaperHelper.GetWallPaper();
			this.classifiedImageList = classifiedImageList;
			FileList = new ObservableCollection<ClassificateListItemData>(this.classifiedImageList);

			this.InitializeComponent();
			this.ExtendsContentIntoTitleBar = true;

			// Make window size can't be changed
			OverlappedPresenter presenter = (OverlappedPresenter)this.AppWindow.Presenter;
			presenter.IsMaximizable = false;
			presenter.IsMinimizable = false;
			presenter.IsResizable = false;
			presenter.SetBorderAndTitleBar(true, false);

			StandardUICommand forwardCommand = new StandardUICommand(StandardUICommandKind.Forward);
			forwardCommand.ExecuteRequested += ForwardCommand_ExecuteRequested;
			SaveButton.Command = forwardCommand;
			ExceptButton.Command = forwardCommand;

			ClassificateWindow_Resize(800, 96);
		}

		private async void ClassificateWindowListView_SelectionChanged(object sender, SelectionChangedEventArgs args)
		{
			if (args.AddedItems.Count == 0)
			{
				return;
			}
			// TODO: Blur the ListView while changing the background
			ClassificateListItemData wallPaperItem = (ClassificateListItemData)args.AddedItems[0];
			SetControlState(false);
			CommandBarPathText.Text = wallPaperItem.FileDescription.FileName;
			await Task.Run(() => WallPaperHelper.SetWallPaper(wallPaperItem.FileDescription.FullPath));
			SetControlState(true);
		}

		private void ClassificateWindowExpander_SizeChanged(object sender, SizeChangedEventArgs args)
		{
			const int WINDOW_HEIGHT = 102;
			const int EXPANDER_COLLAPSED_HEIGHT = 48;
			Expander expander = (Expander)sender;

			double desiredHeight = expander.ActualHeight != EXPANDER_COLLAPSED_HEIGHT
				? WINDOW_HEIGHT + expander.ActualHeight - EXPANDER_COLLAPSED_HEIGHT
				: WINDOW_HEIGHT;
			ClassificateWindow_Resize(800, desiredHeight);
		}

		private void ForwardCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
		{
			ClassificateCommand command = (ClassificateCommand)args.Parameter;
			if (ClassificateListView.SelectedIndex != -1)
			{
				this.selectedIndexCache = ClassificateListView.SelectedIndex;
				ClassificateListItemData oldItem = FileList.ElementAt(ClassificateListView.SelectedIndex);
				ClassificateListItemData newItem = new ClassificateListItemData
				{
					FileDescription = oldItem.FileDescription,
					State = command == ClassificateCommand.Add ? ClassificateState.Save : ClassificateState.Except,
					Symbol = command == ClassificateCommand.Add ? "\uF13E" : "\uF13D"
				};
				FileList[ClassificateListView.SelectedIndex] = newItem;
			}
			ClassificateListView.SelectedIndex = this.selectedIndexCache < (FileList.Count - 1)
				? this.selectedIndexCache + 1
				: this.selectedIndexCache;
		}

		private void SetControlState(bool newControlState)
		{
			ClassificateListView.IsEnabled = newControlState;
			SaveButton.IsEnabled = newControlState;
			ExceptButton.IsEnabled = newControlState;
			PreviousButton.IsEnabled = newControlState ? ClassificateListView.SelectedIndex > 0 : false;
		}

		private void PreviousButton_Click(object sender, RoutedEventArgs e)
		{
			ClassificateListView.SelectedIndex -= 1;
		}

		private async void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: Add condition If desktop background was background color
			await Task.Run(() => WallPaperHelper.SetWallPaper(this.defaultWallPaperPath));

			this.classifiedImageList.Clear();
			this.classifiedImageList.AddRange(FileList);
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

	public class ClassificateListItemData
	{
		public required FileDescription FileDescription { get; set; }
		public required ClassificateState State { get; set; }
		public required string Symbol { get; set; }
	}

	public record struct FileDescription(string FileName, string FullPath, byte[] HashArray, string Size);

	public enum ClassificateCommand
	{
		Add,
		Remove,
	}

	public enum ClassificateState
	{
		None,
		Save,
		Except,
	}
}
