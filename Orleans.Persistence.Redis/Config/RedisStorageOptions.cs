using System;
using System.Collections.Generic;

namespace Orleans.Persistence.Redis.Config
{
	public class RedisStorageOptions
	{
		public IEnumerable<string> Servers { get; set; }
		public int Database { get; set; }
		public bool ThrowExceptionOnInconsistentETag { get; set; } = true;
		public bool HumanReadableSerialization { get; set; }
		public TimeSpan MaxRetryElapsedTimeAllowedMilliseconds { get; set; } = TimeSpan.FromSeconds(3);
		public int ConnectRetry { get; set; } = 25;
		public string Password { get; set; }
		public string KeyPrefix { get; set; } = string.Empty;
		public string ClientName { get; set; }
	}
}
