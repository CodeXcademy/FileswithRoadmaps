// Copyright (c) Files Community
// Licensed under the MIT License.

using Files.App.Data.Items;
using Files.App.Data.Models;
using Files.App.ViewModels.UserControls.Widgets;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.IO;

namespace Files.App.UserControls.Widgets
{
	public sealed partial class RoadmapWidget : UserControl
	{
		public RoadmapWidgetViewModel ViewModel { get; set; } = Ioc.Default.GetRequiredService<RoadmapWidgetViewModel>();

		public RoadmapWidget()
		{
			InitializeComponent();
			Loaded += RoadmapWidget_Loaded;
		}

		private async void RoadmapWidget_Loaded(object sender, RoutedEventArgs e)
		{
			await ViewModel.RefreshWidgetAsync();
		}

		private async void RoadmapSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ViewModel.SelectedRoadmap is not null)
			{
				await ViewModel.SelectRoadmapAsync(ViewModel.SelectedRoadmap.Id);
			}
		}

		private async void AddRoadmapButton_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new ContentDialog
			{
				Title = "Create Roadmap",
				PrimaryButtonText = "Create",
				CloseButtonText = "Cancel",
				DefaultButton = ContentDialogButton.Primary
			};

			var textBox = new TextBox
			{
				PlaceholderText = "Roadmap name"
			};

			dialog.Content = textBox;

			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
			{
				await ViewModel.CreateRoadmapAsync(textBox.Text);
			}
		}

		private async void AddNodeButton_Click(object sender, RoutedEventArgs e)
		{
			if (ViewModel.SelectedRoadmap is null)
			{
				var noRoadmapDialog = new ContentDialog
				{
					Content = "Please create or select a roadmap first.",
					CloseButtonText = "OK"
				};
				await noRoadmapDialog.ShowAsync();
				return;
			}

			var openDialog = new Microsoft.Win32.OpenFileDialog
			{
				Multiselect = true,
				Title = "Select files or folders"
			};

			if (openDialog.ShowDialog() == true)
			{
				foreach (var filePath in openDialog.FileNames)
				{
					await ViewModel.AddFileNodeAsync(filePath);
				}
			}
		}

		private async void NodeButton_Click(object sender, RoutedEventArgs e)
		{
			if (sender is FrameworkElement element && element.Tag is RoadmapNodeItem item)
			{
				await ViewModel.NavigateToPathAsync(item.Path);
			}
		}

		private void NodeButton_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
		{
			if (e.GetCurrentPoint((UIElement)sender).Properties.IsMiddleButtonPressed)
			{
				if (sender is FrameworkElement element && element.Tag is RoadmapNodeItem item)
				{
					_ = ViewModel.OpenInNewTabAsync(item.Path);
				}
				e.Handled = true;
			}
		}

		private void NodeButton_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
		{
			e.Handled = true;
		}

		private async void OpenNode_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is RoadmapNodeItem item)
			{
				await ViewModel.NavigateToPathAsync(item.Path);
			}
		}

		private async void OpenInNewTabNode_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is RoadmapNodeItem item)
			{
				await ViewModel.OpenInNewTabAsync(item.Path);
			}
		}

		private async void OpenLocationNode_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is RoadmapNodeItem item)
			{
				var folderPath = Path.GetDirectoryName(item.Path);
				if (!string.IsNullOrEmpty(folderPath))
				{
					await ViewModel.NavigateToPathAsync(folderPath);
				}
			}
		}

		private void PropertiesNode_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is RoadmapNodeItem item)
			{
				ViewModel.OpenProperties(item.Path);
			}
		}

		private async void RemoveNode_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is RoadmapNodeItem item)
			{
				await ViewModel.RemoveNodeAsync(item.Item.Id);
			}
		}
	}
}
