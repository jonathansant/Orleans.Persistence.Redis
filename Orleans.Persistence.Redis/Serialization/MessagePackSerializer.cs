using System;

namespace Orleans.Persistence.Redis.Serialization
{
	public class MessagePackSerializer : ISerializer
	{
		public byte[] Serialize(object raw, Type type)
			=> MessagePack.MessagePackSerializer.Typeless.Serialize(raw);

		public object Deserialize(byte[] serializedData, Type type)
			=> MessagePack.MessagePackSerializer.Typeless.Deserialize(serializedData);
	}
}