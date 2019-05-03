using Microsoft.Extensions.Logging;
using Orleans.Persistence.Redis.Config;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

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
				var timeAllowedMilliseconds = (int)_options.MaxRetryElapsedTimeAllowedMilliseconds
					.TotalMilliseconds;

				var config = new ConfigurationOptions
				{
					ReconnectRetryPolicy = new LinearRetry(timeAllowedMilliseconds),
					ConnectRetry = _options.ConnectRetry,
					Password = _options.Password,
					ClientName = _options.ClientName,
					Ssl = _options.UseSsl,
					SslHost = _options.SslHost
				};

				foreach (var host in _options.Servers)
				{
					config.EndPoints.Add(host);
				}

				_connection = await ConnectionMultiplexer.ConnectAsync(config);

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