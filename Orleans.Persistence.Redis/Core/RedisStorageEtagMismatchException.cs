using System;

namespace Orleans.Persistence.Redis.Core
{
	public class RedisStorageEtagMismatchException : Exception
	{
		public RedisStorageEtagMismatchException(
			string currentETag,
			string receivedEtag,
			string operation,
			string grainKey
		) : base($"Etag mismatch during {operation} for grain {grainKey}: Expected = {currentETag ?? "null"} Received = {receivedEtag}")
		{

		}

		public RedisStorageEtagMismatchException(
			string currentETag,
			string receivedEtag,
			string operation,
			string grainKey,
			Exception innerException
		) : base(
			$"Etag mismatch during {operation} for grain {grainKey}: Expected = {currentETag ?? "null"} Received = {receivedEtag}",
			innerException
		)
		{
		}
	}
}