// Copyright (c) Files Community
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Files.App.Data.Models
{
	public sealed class RoadmapModel
	{
		[JsonPropertyName("id")]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		[JsonPropertyName("name")]
		public string Name { get; set; } = string.Empty;

		[JsonPropertyName("nodes")]
		public List<RoadmapNodeModel> Nodes { get; set; } = [];

		[JsonPropertyName("createdAt")]
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		[JsonPropertyName("modifiedAt")]
		public DateTime ModifiedAt { get; set; } = DateTime.Now;
	}

	public sealed class RoadmapNodeModel
	{
		[JsonPropertyName("id")]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		[JsonPropertyName("path")]
		public string Path { get; set; } = string.Empty;

		[JsonPropertyName("name")]
		public string Name { get; set; } = string.Empty;

		[JsonPropertyName("isFolder")]
		public bool IsFolder { get; set; }

		[JsonPropertyName("positionX")]
		public double PositionX { get; set; }

		[JsonPropertyName("positionY")]
		public double PositionY { get; set; }

		[JsonPropertyName("connections")]
		public List<string> ConnectedNodeIds { get; set; } = [];
	}
}
