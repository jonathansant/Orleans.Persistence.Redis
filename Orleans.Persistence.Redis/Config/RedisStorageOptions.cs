using Newtonsoft.Json;
using System.Collections.Generic;

namespace Orleans.Persistence.Redis.Config
{
	public class RedisStorageOptions
	{
		public IEnumerable<string> Servers { get; set; }
		public int Database { get; set; }
		public bool DeleteStateOnClear { get; set; } = false;
		public TypeNameHandling? TypeNameHandling { get; set; }
		public bool ThrowExceptionOnInconsistentETag { get; set; } = true;
	}
}
