// Copyright (c) Files Community
// Licensed under the MIT License.

using Files.App.Data.Contracts;
using Files.App.Data.Items;
using Files.App.Data.Models;
using Files.App.Helpers;
using Files.App.Utils.Storage.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.IO;

namespace Files.App.Views
{
	public sealed partial class RoadmapPage : Page
	{
		private readonly IRoadmapService RoadmapService;
		private RoadmapModel? SelectedRoadmap;
		private RoadmapWidgetViewModel ViewModel;

		public RoadmapPage()
		{
			InitializeComponent();
			RoadmapService = Ioc.Default.GetRequiredService<IRoadmapService>();
			ViewModel = Ioc.Default.GetRequiredService<RoadmapWidgetViewModel>();
			Loaded += RoadmapPage_Loaded;
		}

		private async void RoadmapPage_Loaded(object sender, RoutedEventArgs e)
		{
			await ViewModel.RefreshWidgetAsync();
			RoadmapsList.ItemsSource = ViewModel.Roadmaps;
			NodesItemsControl.ItemsSource = ViewModel.Nodes;

			if (ViewModel.Roadmaps.Count > 0)
			{
				RoadmapsList.SelectedIndex = 0;
			}
		}

		private async void RoadmapsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (RoadmapsList.SelectedItem is RoadmapModel roadmap)
			{
				await ViewModel.SelectRoadmapAsync(roadmap.Id);
				SelectedRoadmap = roadmap;
			}
		}

		private async void CreateRoadmap_Click(object sender, RoutedEventArgs e)
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
				RoadmapsList.ItemsSource = null;
				RoadmapsList.ItemsSource = ViewModel.Roadmaps;
				RoadmapsList.SelectedIndex = ViewModel.Roadmaps.Count - 1;
			}
		}

		private async void DeleteRoadmap_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button button && button.Tag is RoadmapModel roadmap)
			{
				await ViewModel.DeleteRoadmapAsync(roadmap.Id);
				RoadmapsList.ItemsSource = null;
				RoadmapsList.ItemsSource = ViewModel.Roadmaps;
			}
		}

		private async void AddNode_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedRoadmap is null)
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

		private void NodeButton_PointerPressed(Microsoft.UI.Xaml.Input.PointerRoutedEventArgs sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
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
