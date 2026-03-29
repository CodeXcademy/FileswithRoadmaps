// Copyright (c) Files Community
// Licensed under the MIT License.

using Files.App.Data.Contracts;
using Files.App.Data.Items;
using Files.App.Data.Models;
using Files.App.Helpers;
using Files.App.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.IO;
using Windows.Storage;

namespace Files.App.ViewModels.UserControls.Widgets
{
	public sealed partial class RoadmapWidgetViewModel : BaseWidgetViewModel, IWidgetViewModel
	{
		private readonly IRoadmapService RoadmapService;

		public ObservableCollection<RoadmapModel> Roadmaps { get; } = [];
		public ObservableCollection<RoadmapNodeItem> Nodes { get; } = [];

		public RoadmapModel? SelectedRoadmap { get; private set; }

		public string WidgetName => nameof(RoadmapWidget);
		public string AutomationProperties => "Roadmap";
		public string WidgetHeader => "Roadmap";
		public bool IsWidgetSettingEnabled => true;
		public bool ShowMenuFlyout => true;
		public MenuFlyoutItem? MenuFlyoutItem => null;

		public RoadmapWidgetViewModel()
		{
			RoadmapService = Ioc.Default.GetRequiredService<IRoadmapService>();

			OpenFileLocationCommand = new RelayCommand<RoadmapNodeItem>(ExecuteOpenFileLocationCommand);
			OpenPropertiesCommand = new RelayCommand<RoadmapNodeItem>(ExecuteOpenPropertiesCommand);
		}

		public async Task RefreshWidgetAsync()
		{
			await MainWindow.Instance.DispatcherQueue.EnqueueOrInvokeAsync(async () =>
			{
				Roadmaps.Clear();
				var roadmaps = await RoadmapService.GetAllRoadmapsAsync();
				foreach (var roadmap in roadmaps)
				{
					Roadmaps.Add(roadmap);
				}

				if (SelectedRoadmap is null && Roadmaps.Count > 0)
				{
					await SelectRoadmapAsync(Roadmaps[0].Id);
				}
			});
		}

		public async Task SelectRoadmapAsync(string roadmapId)
		{
			SelectedRoadmap = await RoadmapService.GetRoadmapAsync(roadmapId);
			await LoadNodesAsync();
		}

		private async Task LoadNodesAsync()
		{
			Nodes.Clear();
			if (SelectedRoadmap is null)
				return;

			foreach (var nodeModel in SelectedRoadmap.Nodes)
			{
				var nodeItem = new RoadmapNodeItem(nodeModel);
				_ = nodeItem.LoadCardThumbnailAsync();
				Nodes.Add(nodeItem);
			}
		}

		public async Task CreateRoadmapAsync(string name)
		{
			var roadmap = new RoadmapModel { Name = name };
			await RoadmapService.SaveRoadmapAsync(roadmap);
			await RefreshWidgetAsync();
			await SelectRoadmapAsync(roadmap.Id);
		}

		public async Task DeleteRoadmapAsync(string roadmapId)
		{
			await RoadmapService.DeleteRoadmapAsync(roadmapId);
			if (SelectedRoadmap?.Id == roadmapId)
			{
				SelectedRoadmap = null;
				Nodes.Clear();
			}
			await RefreshWidgetAsync();
		}

		public async Task AddFileNodeAsync(string filePath)
		{
			if (SelectedRoadmap is null)
				return;

			var isFolder = await IsPathFolderAsync(filePath);
			var name = Path.GetFileName(filePath);
			var position = CalculateNewNodePosition();

			var node = new RoadmapNodeModel
			{
				Path = filePath,
				Name = name,
				IsFolder = isFolder,
				PositionX = position.X,
				PositionY = position.Y
			};

			await RoadmapService.AddNodeToRoadmapAsync(SelectedRoadmap.Id, node);
			await LoadNodesAsync();
		}

		public async Task RemoveNodeAsync(string nodeId)
		{
			if (SelectedRoadmap is null)
				return;

			await RoadmapService.RemoveNodeFromRoadmapAsync(SelectedRoadmap.Id, nodeId);
			await LoadNodesAsync();
		}

		public async Task UpdateNodePositionAsync(string nodeId, double x, double y)
		{
			if (SelectedRoadmap is null)
				return;

			await RoadmapService.UpdateNodePositionAsync(SelectedRoadmap.Id, nodeId, x, y);
		}

		private async Task<bool> IsPathFolderAsync(string path)
		{
			try
			{
				var attr = await FilesystemTasks.Wrap(() => StorageFolder.GetFolderFromPathAsync(path).AsTask());
				return attr is not null;
			}
			catch
			{
				return false;
			}
		}

		private (double X, double Y) CalculateNewNodePosition()
		{
			if (Nodes.Count == 0)
				return (50, 50);

			var maxX = Nodes.Max(n => n.PositionX);
			var maxY = Nodes.Max(n => n.PositionY);

			if (maxX > maxY)
				return (50, maxY + 100);
			return (maxX + 100, 50);
		}

		public async Task NavigateToPathAsync(string path)
		{
			if (ContentPageContext.ShellPage is not null)
			{
				ContentPageContext.ShellPage.NavigateToPath(path);
			}
			else
			{
				await NavigationHelpers.OpenPathInNewTab(path, true);
			}
		}

		public async Task OpenInNewTabAsync(string path)
		{
			await NavigationHelpers.OpenPathInNewTab(path, true);
		}

		public void OpenProperties(string path)
		{
			var itemPath = path;
			var itemName = Path.GetFileName(path);
			var isFolder = Directory.Exists(path);

			ListedItem listedItem = new(path)
			{
				ItemPath = itemPath,
				ItemNameRaw = itemName,
				PrimaryItemAttribute = isFolder ? StorageItemTypes.Folder : StorageItemTypes.File,
			};

			FilePropertiesHelpers.OpenPropertiesWindow(listedItem, ContentPageContext.ShellPage!);
		}

		private void ExecuteOpenFileLocationCommand(RoadmapNodeItem? item)
		{
			if (item is null || string.IsNullOrEmpty(item.Path))
				return;

			var folderPath = Path.GetDirectoryName(item.Path);
			if (!string.IsNullOrEmpty(folderPath))
			{
				_ = NavigateToPathAsync(folderPath);
			}
		}

		private void ExecuteOpenPropertiesCommand(RoadmapNodeItem? item)
		{
			if (item is null || string.IsNullOrEmpty(item.Path))
				return;

			OpenProperties(item.Path);
		}

		public override List<ContextMenuFlyoutItemViewModel> GetItemMenuItems(WidgetCardItem item, bool isPinned, bool isFolder = false)
		{
			var menuItems = new List<ContextMenuFlyoutItemViewModel>();

			menuItems.Add(new ContextMenuFlyoutItemViewModelBuilder(CommandManager.OpenInNewTabFromHome)
			{
				IsVisible = UserSettingsService.GeneralSettingsService.ShowOpenInNewTab
			}.Build());

			menuItems.Add(new ContextMenuFlyoutItemViewModelBuilder(CommandManager.OpenInNewWindowFromHome)
			{
				IsVisible = UserSettingsService.GeneralSettingsService.ShowOpenInNewWindow
			}.Build());

			menuItems.Add(new ContextMenuFlyoutItemViewModelBuilder(CommandManager.OpenFileLocation)
			{
				IsVisible = isFolder || !string.IsNullOrEmpty(Path.GetExtension(item.Path ?? ""))
			}.Build());

			menuItems.Add(new ContextMenuFlyoutItemViewModelBuilder(CommandManager.OpenProperties)
			{
				IsVisible = CommandManager.OpenProperties.IsExecutable
			}.Build());

			menuItems.Add(new ContextMenuFlyoutItemViewModelBuilder(RemoveRecentItemCommand)
			{
				IsVisible = true,
				Item = item
			}.Build());

			return menuItems;
		}
	}
}
