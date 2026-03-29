// Copyright (c) Files Community
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Win32;
using Windows.Win32.UI.Shell;

namespace Files.App.Data.Items
{
	public sealed partial class RoadmapNodeItem : WidgetCardItem, IDisposable
	{
		public string? AutomationProperties { get; set; }

		public new RoadmapNodeModel Item { get; private set; }

		public string? Text { get; set; }

		public bool IsFolder { get; set; }

		public string Tooltip { get; set; }

		public double PositionX { get; set; }
		public double PositionY { get; set; }

		private BitmapImage? _Thumbnail;
		public BitmapImage? Thumbnail { get => _Thumbnail; set => SetProperty(ref _Thumbnail, value); }

		public RoadmapNodeItem(RoadmapNodeModel item)
		{
			Item = item;
			Text = item.Name;
			Path = item.Path;
			IsFolder = item.IsFolder;
			Tooltip = item.Path;
			PositionX = item.PositionX;
			PositionY = item.PositionY;
		}

		public async Task LoadCardThumbnailAsync()
		{
			if (string.IsNullOrEmpty(Path))
				return;

			var thumbnailSize = (int)(Constants.ShellIconSizes.Large * App.AppModel.AppWindowDPI);
			thumbnailSize = Math.Max(1, thumbnailSize);

			var shellItem = await FilesystemTasks.Wrap(() => StorageFileExtensions.GetShellFileItemFromPathAsync(Path));
			if (shellItem is null)
				return;

			shellItem.TryGetThumbnail(thumbnailSize, SIIGBF.SIIGBF_ICONONLY, out var rawThumbnailData);
			if (rawThumbnailData is null)
				return;

			Thumbnail = await rawThumbnailData.ToBitmapAsync();
		}

		public void Dispose()
		{
		}
	}
}
