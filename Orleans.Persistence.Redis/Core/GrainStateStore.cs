﻿using Orleans.Persistence.Redis.Config;
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
		private readonly IHumanReadableSerializer _humanReadableSerializer;
		private readonly RedisStorageOptions _options;

		public GrainStateStore(
			DbConnection connection,
			RedisStorageOptions options,
			ISerializer serializer,
			IHumanReadableSerializer humanReadableSerializer
		)
		{
			_connection = connection;
			_serializer = serializer;
			_humanReadableSerializer = humanReadableSerializer;
			_options = options;
		}

		public async Task<IGrainState> GetGrainState(string grainId, Type stateType)
		{
			var state = await _connection.Database.StringGetAsync(GetKey(grainId));
			if (!state.HasValue)
				return null;

			if (_options.HumanReadableSerialization)
				return (IGrainState)_humanReadableSerializer.Deserialize(state, stateType);

			return (IGrainState)_serializer.Deserialize(state, stateType);
		}

		public async Task UpdateGrainState(string grainId, IGrainState grainState)
		{
			var key = GetKey(grainId);
			var stateType = grainState.GetType();

			if (_options.ThrowExceptionOnInconsistentETag)
			{
				var storedGrainState = await GetGrainState(grainId, stateType);
				ValidateETag(grainState.ETag, storedGrainState?.ETag, stateType.Name);
			}

			grainState.ETag = grainState.State.ComputeHashSync();

			if (_options.HumanReadableSerialization)
				await _connection.Database.StringSetAsync(key, _humanReadableSerializer.Serialize(grainState, stateType));
			else
				await _connection.Database.StringSetAsync(key, _serializer.Serialize(grainState, stateType));
		}

		public Task DeleteGrainState(string grainId, IGrainState grainState)
			=> _connection.Database.KeyDeleteAsync(GetKey(grainId));

		private string GetKey(string grainId)
			=> string.IsNullOrEmpty(_options.KeyPrefix)
				? grainId
				: $"{_options.KeyPrefix}-{grainId}";

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
