using Microsoft.Extensions.Logging;
using Orleans.Persistence.Redis.Utils;
using Orleans.Runtime;
using Orleans.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

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

		public async Task Init(CancellationToken ct)
			=> await _connection.Connect();

		public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
		{
			try
			{
				var primaryKey = grainReference.ToKeyString();
				await _grainStateStore.DeleteGrainState(primaryKey, grainState);
				grainState.Empty();
			}
			catch (Exception ex)
			{
				LogError("clearing", ex, grainType, grainReference, grainReference.ToKeyString());
				throw;
			}
		}

		public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
		{
			var primaryKey = grainReference.ToKeyString();
			try
			{
				var storedState = await _grainStateStore.GetGrainState(primaryKey, grainState.GetType());
				if (storedState != null)
					grainState.From(storedState);
			}
			catch (Exception ex)
			{
				LogError("reading", ex, grainType, grainReference, primaryKey);
				throw;
			}
		}

		public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
		{
			try
			{
				var primaryKey = grainReference.ToKeyString();
				await _grainStateStore.UpdateGrainState(primaryKey, grainState);
			}
			catch (Exception ex)
			{
				LogError("writing", ex, grainType, grainReference, grainReference.ToKeyString());
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

		private void LogError(string op, Exception ex, string grainType, GrainReference grainReference, string primaryKey)
			=> _logger.LogError(
				ex,
				"Error {op} state: GrainType = {grainType} Pk = {primaryKey} GrainId = {grainId} from Database = {db}",
				op,
				primaryKey,
				grainType,
				grainReference,
				_connection.Database
			);
	}
}
