using ByteSizeLib;
using Microsoft.Extensions.Logging;
using Orleans.Persistence.Redis.Compression;
using Orleans.Persistence.Redis.Config;
using Orleans.Persistence.Redis.Serialization;
using Orleans.Persistence.Redis.Utils;
using Orleans.Storage;
using StackExchange.Redis;
using System.Text;
using System.Text.RegularExpressions;

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
		private readonly ICompression _compression;

		public GrainStateStore(
			DbConnection connection,
			RedisStorageOptions options,
			ISerializer serializer,
			IHumanReadableSerializer humanReadableSerializer,
			ILogger<GrainStateStore> logger
		) : this(connection, options, serializer, humanReadableSerializer, logger, null)
		{
		}

		public GrainStateStore(
			DbConnection connection,
			RedisStorageOptions options,
			ISerializer serializer,
			IHumanReadableSerializer humanReadableSerializer,
			ILogger<GrainStateStore> logger,
			ICompression compression
		)
		{
			_connection = connection;
			_serializer = serializer;
			_humanReadableSerializer = humanReadableSerializer;
			_logger = logger;
			_options = options;
			_compression = compression;
		}

		public async Task<IGrainState<T>> GetGrainState<T>(string grainId, Type stateType)
		{
			var key = GetKey(grainId);

			RedisValue state = default;

			if (_options.SegmentSize.HasValue)
				await TimeAction(async () => state = await ReadSegments(key), key, OperationDirection.Read, stateType);
			else
				await TimeAction(async () => state = await _connection.Database.StringGetAsync(key), key, OperationDirection.Read, stateType);

			if (!state.HasValue)
				return null;

			LogDiagnostics(key, state, OperationDirection.Read, stateType);

			if (!_options.HumanReadableSerialization)
				return (IGrainState<T>)_serializer.Deserialize<IGrainState<T>>(state);

			if (_compression != null)
				state = _compression.Decompress(state).GetString();

			return (IGrainState<T>)_humanReadableSerializer.Deserialize(state, stateType);
		}

		public async Task UpdateGrainState<T>(string grainId, IGrainState<T> grainState)
		{
			var key = GetKey(grainId);
			var stateType = grainState.GetType();

			if (_options.ThrowExceptionOnInconsistentETag)
			{
				var storedGrainState = await GetGrainState<T>(grainId, stateType);
				ValidateETag(grainState.ETag, storedGrainState?.ETag, stateType.GetDemystifiedName());
			}

			grainState.ETag = grainState.State.ComputeHashSync();

			if (_options.SegmentSize.HasValue)
			{
				await SaveSegments(key, grainState, stateType);
				return;
			}

			var serializedState = Serialize(grainState, stateType);

			LogDiagnostics(key, serializedState, OperationDirection.Write, stateType);

			await TimeAction(() => _connection.Database.StringSetAsync(key, serializedState), key, OperationDirection.Read, stateType);
		}

		public async Task DeleteGrainState<T>(string grainId, IGrainState<T> grainState)
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
			=> await TimeAction(() => new List<Task>() { action() }, key, direction, grainStateType);

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
				return default;

			if (_options.DeleteOldSegments)
			{
				var segmentSize = _options.SegmentSize!.Value;
				var entries = await _connection.Database.HashGetAllAsync(key);
				var totalBytes = entries.Sum(x => x.Value.Length());
				var buffer = new byte[totalBytes];

				foreach (var e in entries)
				{
					var index = int.Parse(Regex.Match(e.Name!, @"^Segment(\d+)$").Groups[1].Value);
					var temp = Encoding.UTF8.GetBytes(e.Value!);
					Array.Copy(temp, 0, buffer, index * segmentSize, temp.Length);
				}

				return buffer;
			}

			var list = new List<Task<RedisValue>>();
			var tmp = Encoding.UTF8.GetString(await _connection.Database.HashGetAsync(key, "Total"));
			var segments = _humanReadableSerializer.Deserialize<Segments>(tmp);
			for (var i = 0; i < segments.NoOfSegments; i++)
				list.Add(_connection.Database.HashGetAsync(key, $"Segment{i}"));

			var results = await Task.WhenAll(list.ToArray());

			var data = new List<byte>();
			foreach (var r in results)
				data.AddRange(Decode(r));

			return data.ToArray();
		}

		private void AddSegmentToRedis(RedisValue serializedValue, List<HashEntry> entries, long segmentSize, long nSize, ref int segment)
		{
			var tmp = new byte[nSize];
			Array.Copy(serializedValue, segment * segmentSize, tmp, 0, nSize);
			entries.Add(new HashEntry($"Segment{segment}", Encode(tmp)));
			segment++;
		}

		private async Task SaveSegments<T>(string key, IGrainState<T> grainState, Type stateType)
		{
			var segmentSize = _options.SegmentSize!.Value;
			var entries = new List<HashEntry>();
			var segment = 0;

			var serializedState = Serialize(grainState, stateType);
			var totalBytes = serializedState.Length();

			while (totalBytes > segmentSize)
			{
				AddSegmentToRedis(serializedState, entries, segmentSize, segmentSize, ref segment);
				totalBytes -= segmentSize;
			}

			if (totalBytes > 0)
			{
				AddSegmentToRedis(serializedState, entries, segmentSize, totalBytes, ref segment);
			}

			if (!_options.DeleteOldSegments)
			{
				var bytes = Encoding.UTF8.GetBytes(_humanReadableSerializer.Serialize<Segments>(new Segments()
				{
					NoOfSegments = segment
				}));

				entries.Add(new HashEntry("Total", bytes));

				await TimeAction(() => _connection.Database.HashSetAsync(key, entries.ToArray()), key, OperationDirection.Read, stateType);
				return;
			}

			var pendingList = new List<Task>
			{
				_connection.Database.HashSetAsync(key, entries.ToArray())
			};

			while (await _connection.Database.HashExistsAsync(key, $"Segment{segment}"))
			{
				pendingList.Add(_connection.Database.HashDeleteAsync(key, $"Segment{segment}"));
				segment++;
			}
			await TimeAction(() => pendingList, key, OperationDirection.Read, stateType);
		}

		private string Encode(RedisValue value)
			=> (_options.HumanReadableSerialization && _compression == null) ? value : Convert.ToBase64String(value);

		private byte[] Decode(RedisValue value)
			=> (_options.HumanReadableSerialization && _compression == null) ? value : Convert.FromBase64String(value);

		private RedisValue Serialize<T>(IGrainState<T> grainState, Type stateType)
		{
			RedisValue serializedState;
			if (!_options.HumanReadableSerialization)
				return _serializer.Serialize(grainState);

			var serialized = _humanReadableSerializer.Serialize(grainState, stateType);
			return _compression != null
				? _compression.Compress(serialized.GetBytes())
				: serialized;
		}
	}

	internal static class StringExtensions
	{
		public static byte[] GetBytes(this string str)
			=> Encoding.UTF8.GetBytes(str);

		public static string GetString(this byte[] buffer)
			=> Encoding.UTF8.GetString(buffer);
	}

	internal enum OperationDirection
	{
		Write,
		Read,
		Delete,
	}
}
