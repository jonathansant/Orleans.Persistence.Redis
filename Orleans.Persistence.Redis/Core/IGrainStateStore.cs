using System;
using System.Threading.Tasks;

namespace Orleans.Persistence.Redis.Core
{
	public interface IGrainStateStore
	{
		Task<IGrainState<T>> GetGrainState<T>(string grainId, Type stateType);
		Task UpdateGrainState<T>(string grainId, IGrainState<T> grainState);
		Task DeleteGrainState<T>(string grainId, IGrainState<T> grainState);
	}
}