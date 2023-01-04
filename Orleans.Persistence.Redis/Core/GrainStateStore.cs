using ByteSizeLib;
using Microsoft.Extensions.Logging;
using Orleans.Persistence.Redis.Config;
using Orleans.Persistence.Redis.Serialization;
using Orleans.Persistence.Redis.Utils;
using Orleans.Storage;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Orleans.Persistence.Redis.Core
{
	internal class Segments
	{
		public int NoOfSegments { get; set; }
	}

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

			if (_options.SegmentSize.HasValue)
			{
				await TimeAction(async () => state = await ReadSegments(key), key, OperationDirection.Read, stateType);
			}
			else
			{
				await TimeAction(async () => state = await _connection.Database.StringGetAsync(key), key, OperationDirection.Read, stateType);
			}

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

			if (_options.SegmentSize.HasValue)
			{
				await SaveSegments(key, grainState, stateType);
				return;
			}


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
		{
			if (_options.KeyBuilder != null)
				return _options.KeyBuilder(grainId);
			return string.IsNullOrEmpty(_options.KeyPrefix)
				? grainId
				: $"{_options.KeyPrefix}-{grainId}";
		}

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

		private async Task TimeAction(Func<List<Task>> action, string key, OperationDirection direction, Type grainStateType)
		{
			if (!_logger.IsEnabled(LogLevel.Warning))
			{
				await Task.WhenAll(action());
				return;
			}

			var watch = ValueStopwatch.StartNew();

			await Task.WhenAll(action());

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

		private async Task<RedisValue> ReadSegments(string key)
		{
			if (!await _connection.Database.KeyExistsAsync(key))
			{
				return default;
			}

			if (_options.DeleteOldSegments)
			{
				var max = _options.SegmentSize!.Value;
				var entries = await _connection.Database.HashGetAllAsync(key);
				var n = entries.Sum(x => x.Value.Length());
				var buffer = new byte[n];

				foreach (var o in entries)
				{
					var index = int.Parse(Regex.Match(o.Name!, @"^Segment(\d+)$").Groups[1].Value);
					var temp = Encoding.UTF8.GetBytes(o.Value!);
					Array.Copy(temp, 0, buffer, index * max, temp.Length);
				}

				return buffer;
			}

			var list = new List<Task<RedisValue>>();
			var s = Encoding.UTF8.GetString(await _connection.Database.HashGetAsync(key, "Total"));
			var segments = _humanReadableSerializer.Deserialize<Segments>(s);
			for (var i = 0; i < segments.NoOfSegments; i++)
			{
				list.Add(_connection.Database.HashGetAsync(key, $"Segment{i}"));
			}

			var results = await Task.WhenAll(list.ToArray());
			var data = new List<byte>();
			foreach (var r in results)
			{
				data.AddRange((byte[])r);
			}

			return data.ToArray();
		}


		private async Task SaveSegments(string key, IGrainState grainState, Type stateType)
		{
			var max = _options.SegmentSize!.Value;
			var entries = new List<HashEntry>();
			var segment = 0;

			RedisValue serializedState = _options.HumanReadableSerialization
				? _humanReadableSerializer.Serialize(grainState, stateType)
				: _serializer.Serialize(grainState, stateType);

			var n = serializedState.Length();
			var tmp = new byte[max];
			while (n > max)
			{
				Array.Copy(serializedState, segment * max, tmp, 0, max);
				entries.Add(new HashEntry($"Segment{segment}", tmp));
				segment++;
				n -= max;
			}

			if (n > 0)
			{
				Array.Copy(serializedState, segment * max, tmp, 0, n);
				entries.Add(new HashEntry($"Segment{segment}", tmp));
				segment++;
			}

			if (!_options.DeleteOldSegments)
			{

				var segments = new Segments()
				{
					NoOfSegments = segment
				};

				var bytes = Encoding.UTF8.GetBytes(_humanReadableSerializer.Serialize<Segments>(segments));

				entries.Add(new HashEntry("Total", bytes));

				await TimeAction(() => _connection.Database.HashSetAsync(key, entries.ToArray()), key, OperationDirection.Read, stateType);
				return;
			}

			var pending = new List<Task>
			{
				_connection.Database.HashSetAsync(key, entries.ToArray())
			};

			while (await _connection.Database.HashExistsAsync(key, $"Segment{segment}"))
			{
				pending.Add(_connection.Database.HashDeleteAsync(key, $"Segment{segment}"));
				segment++;
			}
			await TimeAction(() => pending, key, OperationDirection.Read, stateType);
		}
	}

	internal enum OperationDirection
	{
		Write,
		Read,
		Delete,
	}
}
