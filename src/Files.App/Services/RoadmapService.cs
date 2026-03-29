// Copyright (c) Files Community
// Licensed under the MIT License.

using Files.App.Data.Contracts;
using Files.App.Data.Models;
using System.IO;
using System.Text.Json;
using Windows.Storage;

namespace Files.App.Services
{
	internal sealed class RoadmapService : IRoadmapService
	{
		private readonly string _roadmapsFilePath;
		private List<RoadmapModel> _roadmaps = [];
		private readonly JsonSerializerOptions _jsonOptions;

		public RoadmapService()
		{
			var localFolder = ApplicationData.Current.LocalFolder.Path;
			_roadmapsFilePath = Path.Combine(localFolder, "roadmaps.json");
			_jsonOptions = new JsonSerializerOptions { WriteIndented = true };
			_ = LoadRoadmapsAsync();
		}

		private async Task LoadRoadmapsAsync()
		{
			try
			{
				if (File.Exists(_roadmapsFilePath))
				{
					var json = await File.ReadAllTextAsync(_roadmapsFilePath);
					_roadmaps = JsonSerializer.Deserialize<List<RoadmapModel>>(json, _jsonOptions) ?? [];
				}
			}
			catch
			{
				_roadmaps = [];
			}
		}

		private async Task SaveRoadmapsToFileAsync()
		{
			var json = JsonSerializer.Serialize(_roadmaps, _jsonOptions);
			await File.WriteAllTextAsync(_roadmapsFilePath, json);
		}

		public Task<IEnumerable<RoadmapModel>> GetAllRoadmapsAsync()
		{
			return Task.FromResult<IEnumerable<RoadmapModel>>(_roadmaps);
		}

		public Task<RoadmapModel?> GetRoadmapAsync(string id)
		{
			var roadmap = _roadmaps.FirstOrDefault(r => r.Id == id);
			return Task.FromResult(roadmap);
		}

		public async Task SaveRoadmapAsync(RoadmapModel roadmap)
		{
			var existing = _roadmaps.FirstOrDefault(r => r.Id == roadmap.Id);
			if (existing is not null)
			{
				_roadmaps.Remove(existing);
			}
			roadmap.ModifiedAt = DateTime.Now;
			_roadmaps.Add(roadmap);
			await SaveRoadmapsToFileAsync();
		}

		public async Task DeleteRoadmapAsync(string id)
		{
			var roadmap = _roadmaps.FirstOrDefault(r => r.Id == id);
			if (roadmap is not null)
			{
				_roadmaps.Remove(roadmap);
				await SaveRoadmapsToFileAsync();
			}
		}

		public async Task AddNodeToRoadmapAsync(string roadmapId, RoadmapNodeModel node)
		{
			var roadmap = _roadmaps.FirstOrDefault(r => r.Id == roadmapId);
			if (roadmap is not null)
			{
				roadmap.Nodes.Add(node);
				roadmap.ModifiedAt = DateTime.Now;
				await SaveRoadmapsToFileAsync();
			}
		}

		public async Task RemoveNodeFromRoadmapAsync(string roadmapId, string nodeId)
		{
			var roadmap = _roadmaps.FirstOrDefault(r => r.Id == roadmapId);
			var node = roadmap?.Nodes.FirstOrDefault(n => n.Id == nodeId);
			if (roadmap is not null && node is not null)
			{
				roadmap.Nodes.Remove(node);
				foreach (var n in roadmap.Nodes)
				{
					n.ConnectedNodeIds.Remove(nodeId);
				}
				roadmap.ModifiedAt = DateTime.Now;
				await SaveRoadmapsToFileAsync();
			}
		}

		public async Task UpdateNodePositionAsync(string roadmapId, string nodeId, double x, double y)
		{
			var roadmap = _roadmaps.FirstOrDefault(r => r.Id == roadmapId);
			var node = roadmap?.Nodes.FirstOrDefault(n => n.Id == nodeId);
			if (node is not null)
			{
				node.PositionX = x;
				node.PositionY = y;
				if (roadmap is not null)
				{
					roadmap.ModifiedAt = DateTime.Now;
				}
				await SaveRoadmapsToFileAsync();
			}
		}
	}
}
