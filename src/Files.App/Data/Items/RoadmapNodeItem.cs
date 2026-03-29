// Copyright (c) Files Community
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Media.Imaging;

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

		public BitmapImage? Thumbnail { get; set; }

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

		public Task LoadCardThumbnailAsync()
		{
			return Task.CompletedTask;
		}

		public void Dispose()
		{
		}
	}
}
