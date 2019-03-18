using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Persistence.Redis.Config;
using StackExchange.Redis;

namespace Orleans.Persistence.Redis.Core
{
	public class DbConnection : IDisposable
	{
		private readonly ILogger<DbConnection> _logger;
		private readonly RedisStorageOptions _options;
		private ConnectionMultiplexer _connection;

		public IDatabaseAsync Database { get; private set; }

		public DbConnection(
			RedisStorageOptions options,
			ILogger<DbConnection> logger
		)
		{
			_logger = logger;
			_options = options;
		}

		public async Task Connect()
		{
			try
			{
				_connection = await ConnectionMultiplexer.ConnectAsync(
					string.Join(",", _options.Servers)
				);

				Database = _connection.GetDatabase(_options.Database);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Failed to connect: {@servers} Db: {@db}",
					_options.Servers,
					_options.Database
				);
				throw;
			}
		}

		public void Dispose()
		{
			if (_connection == null)
				return;

			_connection.Close();
			_connection.Dispose();
		}
	}
}