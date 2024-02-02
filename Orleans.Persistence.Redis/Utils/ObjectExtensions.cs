﻿using Newtonsoft.Json;
using System.Data.HashFunction.xxHash;
using System.Text;

namespace Orleans.Persistence.Redis.Utils
{
	internal static class ObjectExtensions
	{
		private static readonly IxxHash HashFunction = xxHashFactory.Instance.Create();

		public static async Task<string> ComputeHash(this object obj)
		{
			var serialized = JsonConvert.SerializeObject(obj);
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized)))
			{
				var value = await HashFunction.ComputeHashAsync(stream);
				return value.AsBase64String();
			}
		}

		public static string ComputeHashSync(this object obj)
		{
			var serialized = JsonConvert.SerializeObject(obj);
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized)))
			{
				var value = HashFunction.ComputeHash(stream);
				return value.AsBase64String();
			}
		}

	}
}
