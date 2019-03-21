using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Orleans.Hosting;
using Orleans.Runtime;
using System;

namespace Orleans.Persistence.Redis.Serialization
{
	public static class MessagePackSerializationExtension
	{
		public static ISiloHostBuilder UseRedisMessagePackSerializer(this ISiloHostBuilder siloHostBuilder, string name)
			=> siloHostBuilder.ConfigureServices((builder, serviceCollection)
				=> serviceCollection.AddSingletonNamedService<ISerializer>(name,
					(provider, n) => ActivatorUtilities.CreateInstance<MessagePackSerializer>(provider)));
	}

	public class MessagePackSerializer : ISerializer
	{
		public byte[] Serialize(object raw) 
			=> MessagePack.MessagePackSerializer.Typeless.Serialize(raw);

		public string SerializeToString(object raw)
			=> MessagePack.MessagePackSerializer.ToJson(raw);

		public object Deserialize(byte[] serializedData, Type type) 
			=> MessagePack.MessagePackSerializer.Typeless.Deserialize(serializedData);

		public object Deserialize(string serializedData, Type type)
			=> JsonConvert.DeserializeObject(serializedData, type);
	}
}