using System;
using System.Threading.Tasks;

namespace Orleans.Persistence.Redis.Core
{
	public interface IGrainStateStore
	{
		Task<IGrainState> GetGrainState(string grainId, Type stateType);
		Task UpdateGrainState(string grainId, IGrainState grainState);
		Task DeleteGrainState(string grainId, IGrainState grainState);
	}
}