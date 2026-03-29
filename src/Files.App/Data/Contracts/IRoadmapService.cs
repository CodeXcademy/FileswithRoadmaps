// Copyright (c) Files Community
// Licensed under the MIT License.

namespace Files.App.Data.Contracts
{
	public interface IRoadmapService
	{
		Task<IEnumerable<RoadmapModel>> GetAllRoadmapsAsync();
		Task<RoadmapModel?> GetRoadmapAsync(string id);
		Task SaveRoadmapAsync(RoadmapModel roadmap);
		Task DeleteRoadmapAsync(string id);
		Task AddNodeToRoadmapAsync(string roadmapId, RoadmapNodeModel node);
		Task RemoveNodeFromRoadmapAsync(string roadmapId, string nodeId);
		Task UpdateNodePositionAsync(string roadmapId, string nodeId, double x, double y);
	}
}
