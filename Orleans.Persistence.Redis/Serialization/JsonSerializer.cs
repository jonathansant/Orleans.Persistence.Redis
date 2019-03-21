using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Orleans.Hosting;
using Orleans.Runtime;
using System;
using System.IO;

namespace Orleans.Persistence.Redis.Serialization
{
	public static class SerializationExtension
	{
		public static ISiloHostBuilder UseRedisJsonSerializer(this ISiloHostBuilder siloHostBuilder, string name, JsonSerializerSettings settings)
			=> siloHostBuilder.ConfigureServices((builder, serviceCollection)
				=> serviceCollection.AddSingletonNamedService<ISerializer>(name,
					(provider, n) => ActivatorUtilities.CreateInstance<JsonSerializer>(provider, settings)));
	}

	public class JsonSerializer : ISerializer
	{
		private readonly JsonSerializerSettings _settings;

		public JsonSerializer(
			JsonSerializerSettings settings
		)
		{
			_settings = settings;
		}

		public byte[] Serialize(object raw)
		{
			using (var memoryStream = new MemoryStream())
			using (var writer = new BsonDataWriter(memoryStream))
			{
				var serializer = new Newtonsoft.Json.JsonSerializer();
				serializer.Serialize(writer, raw);
				return memoryStream.ToArray();
			}
		}

		public string SerializeToString(object raw)
			=> JsonConvert.SerializeObject(raw, _settings);

		public object Deserialize(byte[] serializedData, Type type)
		{
			using (var stream = new MemoryStream(serializedData))
			using (var reader = new BsonReader(stream))
			{
				var serializer = new Newtonsoft.Json.JsonSerializer();
				return serializer.Deserialize(reader, type);
			}
		}

		public object Deserialize(string serializedData, Type type)
			=> JsonConvert.DeserializeObject(serializedData, type, _settings);
	}
}