using Orleans.Persistence.Redis.Config;
using Orleans.Persistence.Redis.Serialization;
using Orleans.Persistence.Redis.Utils;
using Orleans.Storage;
using System;
using System.Threading.Tasks;

namespace Orleans.Persistence.Redis.Core
{
	internal class GrainStateStore : IGrainStateStore
	{
		private readonly DbConnection _connection;
		private readonly ISerializer _serializer;
		private readonly RedisStorageOptions _options;

		public GrainStateStore(
			DbConnection connection,
			RedisStorageOptions options,
			ISerializer serializer
		)
		{
			_connection = connection;
			_serializer = serializer;
			_options = options;
		}

		public async Task<IGrainState> GetGrainState(string grainId, Type stateType)
		{
			var state = await _connection.Database.StringGetAsync(grainId);
			if (!state.HasValue)
				return null;

			if (_options.PlainTextSerialization)
				return (IGrainState)_serializer.Deserialize((string)state, stateType);

			return (IGrainState)_serializer.Deserialize((byte[])state, stateType);
		}

		public async Task UpdateGrainState(string grainId, IGrainState grainState)
		{
			if (_options.ThrowExceptionOnInconsistentETag)
			{
				var storedGrainState = await GetGrainState(grainId, grainState.GetType());
				ValidateETag(grainState.ETag, storedGrainState?.ETag, grainState.GetType().Name);
			}

			grainState.ETag = grainState.State.ComputeHashSync();

			if (_options.PlainTextSerialization)
				await _connection.Database.StringSetAsync(grainId, _serializer.SerializeToString(grainState));
			else
				await _connection.Database.StringSetAsync(grainId, _serializer.Serialize(grainState));
		}

		public Task DeleteGrainState(string grainId, IGrainState grainState)
			=> _connection.Database.KeyDeleteAsync(grainId);

		private static void ValidateETag(string currentETag, string storedEtag, string typeName)
		{
			if (currentETag == null && storedEtag == null)
				return;

			if (storedEtag == null)
				ThrowInconsistentState(currentETag, storedEtag, typeName);

			if (storedEtag == currentETag)
				return;

			ThrowInconsistentState(currentETag, storedEtag, typeName);
		}

		private static void ThrowInconsistentState(string currentETag, string storedEtag, string typeName)
			=> throw new InconsistentStateException(
				$"Inconsistent state detected while performing write operations for type:{typeName}.",
				storedEtag,
				currentETag
			);
	}
}
