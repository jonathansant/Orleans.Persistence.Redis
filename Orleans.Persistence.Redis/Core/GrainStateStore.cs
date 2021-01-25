using ByteSizeLib;
using Microsoft.Extensions.Logging;
using Orleans.Persistence.Redis.Config;
using Orleans.Persistence.Redis.Serialization;
using Orleans.Persistence.Redis.Utils;
using Orleans.Storage;
using StackExchange.Redis;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Persistence.Redis.Core
{
	internal class GrainStateStore : IGrainStateStore
	{
		private readonly DbConnection _connection;
		private readonly ISerializer _serializer;
		private readonly IHumanReadableSerializer _humanReadableSerializer;
		private readonly ILogger<GrainStateStore> _logger;
		private readonly RedisStorageOptions _options;

		public GrainStateStore(
			DbConnection connection,
			RedisStorageOptions options,
			ISerializer serializer,
			IHumanReadableSerializer humanReadableSerializer,
			ILogger<GrainStateStore> logger
		)
		{
			_connection = connection;
			_serializer = serializer;
			_humanReadableSerializer = humanReadableSerializer;
			_logger = logger;
			_options = options;
		}

		public async Task<IGrainState> GetGrainState(string grainId, Type stateType)
		{
			var key = GetKey(grainId);

			RedisValue state = default;
			await TimeAction(async () => state = await _connection.Database.StringGetAsync(key), key, OperationDirection.Read, stateType);

			if (!state.HasValue)
				return null;

			LogDiagnostics(key, state, OperationDirection.Read, stateType);

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
				ValidateETag(grainState.ETag, storedGrainState?.ETag, stateType.GetDemystifiedName());
			}

			grainState.ETag = grainState.State.ComputeHashSync();

			RedisValue serializedState = _options.HumanReadableSerialization
				? _humanReadableSerializer.Serialize(grainState, stateType)
				: _serializer.Serialize(grainState, stateType);

			LogDiagnostics(key, serializedState, OperationDirection.Write, stateType);

			await TimeAction(() => _connection.Database.StringSetAsync(key, serializedState), key, OperationDirection.Read, stateType);
		}

		public async Task DeleteGrainState(string grainId, IGrainState grainState)
		{
			var key = GetKey(grainId);
			var stateType = grainState.GetType();
			await TimeAction(() => _connection.Database.KeyDeleteAsync(key), key, OperationDirection.Delete, stateType);
		}

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
				$"Inconsistent state detected while performing write operations for type: {typeName}.",
				storedEtag,
				currentETag
			);

		private async Task TimeAction(Func<Task> action, string key, OperationDirection direction, Type grainStateType)
		{
			if (!_logger.IsEnabled(LogLevel.Warning))
			{
				await action();
				return;
			}

			var watch = ValueStopwatch.StartNew();

			await action();

			var stopwatchElapsed = watch.GetElapsedTime();
			var threshold = _options.ExecutionDurationWarnThreshold;
			if (_logger.IsEnabled(LogLevel.Warning) && stopwatchElapsed > threshold)
				_logger.LogWarning(
				"Redis operation took longer than threshold {elapsed:n0}ms/{thresholdDuration:n0}ms. Key: {redisKey}, Type: {grainStateType}, Direction: {direction}",
				stopwatchElapsed.TotalMilliseconds,
				threshold.TotalMilliseconds,
				key,
				grainStateType.GetDemystifiedName(),
				direction
			);
		}

		private void LogDiagnostics(string key, RedisValue serializedState, OperationDirection direction, Type grainStateType)
		{
			if (!_logger.IsEnabled(LogLevel.Warning))
				return;
			var stateBytes = (byte[])serializedState;
			var stateSize = ByteSize.FromBytes(stateBytes.Length);
			var keySize = ByteSize.FromBytes(Encoding.UTF8.GetByteCount(key));

			if (stateSize.MebiBytes > _options.FieldSizeWarningThresholdInMb)
				_logger.LogWarning(
					"Redis value exceeds threshold {size}MB/{threshold}MB. Key: {redisKey}, Data Type: {dataType}, Type: {grainStateType}, Direction: {direction}",
					Math.Round(stateSize.MebiBytes, 2),
					_options.FieldSizeWarningThresholdInMb,
					key,
					"GrainState",
					grainStateType.GetDemystifiedName(),
					direction
				);

			if (keySize.MebiBytes > _options.FieldSizeWarningThresholdInMb)
				_logger.LogWarning(
					"Redis value exceeds threshold {size}MB/{threshold}MB. Key: {redisKey}, Data Type: {dataType}, Direction: {direction}",
					Math.Round(stateSize.MebiBytes, 2),
					_options.FieldSizeWarningThresholdInMb,
					key,
					"Key",
					direction
				);
		}
	}

	internal enum OperationDirection
	{
		Write,
		Read,
		Delete,
	}
}
