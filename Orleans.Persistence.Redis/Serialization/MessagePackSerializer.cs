using System;

namespace Orleans.Persistence.Redis.Serialization
{
	public class MessagePackSerializer : ISerializer
	{
		public byte[] Serialize(object raw)
			=> MessagePack.MessagePackSerializer.Typeless.Serialize(raw);

		public object Deserialize<T>(byte[] serializedData)
			=> MessagePack.MessagePackSerializer.Typeless.Deserialize(serializedData);
	}
}