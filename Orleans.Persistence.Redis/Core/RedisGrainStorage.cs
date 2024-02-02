using Microsoft.Extensions.Logging;
using Orleans.Persistence.Redis.Utils;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleans.Persistence.Redis.Core
{
	public class RedisGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
	{
		private readonly string _name;
		private readonly IGrainStateStore _grainStateStore;
		private readonly DbConnection _connection;
		private readonly ILogger<RedisGrainStorage> _logger;

		public RedisGrainStorage(
			string name,
			IGrainStateStore grainStateStore,
			DbConnection connection,
			ILogger<RedisGrainStorage> logger
		)
		{
			_name = name;
			_grainStateStore = grainStateStore;
			_connection = connection;
			_logger = logger;
		}

		public Task Init(CancellationToken ct)
			=> _connection.Connect();

		public async Task ClearStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
		{
			var primaryKey = grainId.ToString();
			try
			{
				await _grainStateStore.DeleteGrainState(primaryKey, grainState);
				grainState.Empty();
			}
			catch (Exception ex)
			{
				LogError("clearing", ex, grainType, grainId, primaryKey);
				throw;
			}
		}

		public async Task ReadStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
		{
			var primaryKey = grainId.ToString();
			try
			{
				var storedState = await _grainStateStore.GetGrainState<T>(primaryKey, grainState.GetType());
				if (storedState != null)
					grainState.From<T>(storedState);
			}
			catch (Exception ex)
			{
				LogError("reading", ex, grainType, grainId, primaryKey);
				throw;
			}
		}

		public async Task WriteStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
		{
			var primaryKey = grainId.ToString();

			try
			{
				await _grainStateStore.UpdateGrainState(primaryKey, grainState);
			}
			catch (Exception ex)
			{
				LogError("writing", ex, grainType, grainId, primaryKey);
				throw;
			}
		}

		public void Participate(ISiloLifecycle lifecycle)
			=> lifecycle.Subscribe(
				OptionFormattingUtilities.Name<RedisGrainStorage>(_name),
				ServiceLifecycleStage.ApplicationServices,
				Init,
				Close
			);

		public Task Close(CancellationToken ct)
		{
			_connection.Dispose();
			return Task.CompletedTask;
		}

		private void LogError(string op, Exception ex, string grainType, GrainId grainId, string primaryKey)
			=> _logger.LogError(
				ex,
				"Error {op} state. GrainType: {grainType}, PK: {primaryKey}, GrainId: {grainId} from Database: {db}",
				op,
				grainType,
				primaryKey,
				grainId,
				_connection.Database
			);
	}
}
