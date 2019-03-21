using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Runtime;
using System;

namespace Orleans.Persistence.Redis.Serialization
{
	public class MessagePackSerializer : ISerializer
	{
		public byte[] Serialize(object raw)
			=> MessagePack.MessagePackSerializer.Typeless.Serialize(raw);

		public object Deserialize(byte[] serializedData, Type type)
			=> MessagePack.MessagePackSerializer.Typeless.Deserialize(serializedData);
	}
}