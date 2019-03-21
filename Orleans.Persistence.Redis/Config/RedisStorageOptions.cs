using System.Collections.Generic;

namespace Orleans.Persistence.Redis.Config
{
	public class RedisStorageOptions
	{
		public IEnumerable<string> Servers { get; set; }
		public int Database { get; set; }
		public bool ThrowExceptionOnInconsistentETag { get; set; } = true;
		public bool PlainTextSerialization { get; set; }
	}
}
