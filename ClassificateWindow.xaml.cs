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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using WallPaperClassificator.Util;
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

		public List<ClassificateListItemData> FileList = [];

		private readonly string unclassifiedImageDirPath;
		private readonly string saveDirPath;
		private readonly string defaultWallPaperPath;
		private readonly List<ClassificateListItemData> classifiedImageList;
		private readonly List<string> acceptableImgExtList = [".jpg", ".jpeg", ".bmp", ".png", ".jfif", ".gif", ".tif", ".tiff"];

		private int posWinX = 0, posWinY = 0, posPressedX = 0, posPressedY = 0;
		private bool pointerMoving = false;
		private int selectedIndexCache = -1; // This value will contains the index of the item that was selected before the refresh request.
		private bool isClassificationFinished = false;
		private ClassificateCommand nextClassificateCommand = ClassificateCommand.Add;

		public ClassificateWindow(string unclassifiedImageDirPath, string saveDirPath, List<ClassificateListItemData> classifiedImageList)
		{
			this.unclassifiedImageDirPath = unclassifiedImageDirPath;
			this.saveDirPath = saveDirPath;
			this.defaultWallPaperPath = WallPaperHelper.GetWallPaper();
			this.classifiedImageList = classifiedImageList;

			this.InitializeComponent();
			this.ExtendsContentIntoTitleBar = true;

			// Make window size can't be changed
			OverlappedPresenter presenter = (OverlappedPresenter)this.AppWindow.Presenter;
			presenter.IsMaximizable = false;
			presenter.IsMinimizable = false;
			presenter.IsResizable = false;
			presenter.SetBorderAndTitleBar(true, false);

			DirectoryInfo unclassifiedDir = new DirectoryInfo(unclassifiedImageDirPath);
			unclassifiedDir.EnumerateFiles()
				.Where(file => acceptableImgExtList.Contains(file.Extension.ToLower())) // TODO: Check if the file is image.
				.ToList()
				.ForEach(file => FileList.Add(new ClassificateListItemData {
					FileName = file.Name,
					State = ClassificateState.None,
					Symbol = "\uF141" // hyphen symbol
				}));
			FileList.TrimExcess();

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
			ClassificateListItemData wallPaperItem = (ClassificateListItemData)args.AddedItems[0];
			SetButtonState(false);
			CommandBarPathText.Text = wallPaperItem.FileName;
			await Task.Run(() => WallPaperHelper.SetWallPaper(Path.Combine(this.unclassifiedImageDirPath, wallPaperItem.FileName)));
			SetButtonState(true);
		}

		private void ClassificateWindowExpander_SizeChanged(object sender, SizeChangedEventArgs args)
		{
			const int EXPANDER_COLLAPSED_HEIGHT = 48;
			Expander expander = (Expander)sender;

			double desiredHeight = expander.ActualHeight != EXPANDER_COLLAPSED_HEIGHT
				? 96 + expander.ActualHeight - EXPANDER_COLLAPSED_HEIGHT
				: 96; // 96 is the command bar height
			ClassificateWindow_Resize(800, desiredHeight);
		}

		private void ClassificateRefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			Deferral refreshCompletionDeferral = args.GetDeferral();

			// Disable buttons while refreshing
			SetButtonState(false);
			this.selectedIndexCache = ClassificateListView.SelectedIndex;
			ClassificateListItemData oldItem = FileList.ElementAt(ClassificateListView.SelectedIndex);
			ClassificateListItemData newItem = default!;

			switch (this.nextClassificateCommand)
			{
				case ClassificateCommand.Add:
					newItem = new ClassificateListItemData {
						FileName = oldItem.FileName,
						State = ClassificateState.Save,
						Symbol = "\uF13E"
					}; // Check symbol
					break;
				case ClassificateCommand.Remove:
					newItem = new ClassificateListItemData {
						FileName = oldItem.FileName,
						State = ClassificateState.Except,
						Symbol = "\uF13D"
					}; // Check symbol
					break;
			}
			ReplaceToNewListViewItem(ClassificateListView.SelectedIndex, newItem);

			isClassificationFinished = ClassificateListView.SelectedIndex == FileList.Count - 1;
			if (isClassificationFinished)
			{
				CloseClassificateWindowConfirmation.Text = "Classificate Progress will be reflected to App Main Window. Do you want to close the window? ";
			}

			SetButtonState(true);
			
			refreshCompletionDeferral.Complete();
			refreshCompletionDeferral.Dispose();
		}

		private void ForwardCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
		{
			ClassificateCommand command = (ClassificateCommand)args.Parameter;
			if (ClassificateListView.SelectedIndex != -1)
			{
				this.nextClassificateCommand = command;
				ClassificateRefreshContainer.RequestRefresh();
			}
			ClassificateListView.SelectedIndex = this.selectedIndexCache < (FileList.Count - 1)
				? this.selectedIndexCache + 1
				: this.selectedIndexCache;
		}

		private void ReplaceToNewListViewItem(int index, ClassificateListItemData newItem)
		{
			// ItemsSource won't be updated if new enumerables is similar to the old one.
			List<ClassificateListItemData> updatedFileList = new List<ClassificateListItemData>(FileList);
			updatedFileList[index] = newItem;
			ClassificateListView.ItemsSource = updatedFileList;
		}

		private void SetButtonState(bool newButtonState)
		{
			SaveButton.IsEnabled = newButtonState;
			ExceptButton.IsEnabled = newButtonState;
			PreviousButton.IsEnabled = newButtonState ? ClassificateListView.SelectedIndex > 0 : false;
		}

		private void PreviousButton_Click(object sender, RoutedEventArgs e)
		{
			ClassificateListView.SelectedIndex -= 1;
		}

		private async void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: Add condition If desktop background was background color
			await Task.Run(() => WallPaperHelper.SetWallPaper(this.defaultWallPaperPath));

			if (isClassificationFinished)
			{
				this.classifiedImageList.Clear();
				this.classifiedImageList.AddRange(FileList);
			}
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
		public required string FileName { get; set; }
		public required ClassificateState State { get; set; }
		public required string Symbol { get; set; }
	}

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
