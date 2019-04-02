using Orleans.Serialization;
using System;

namespace Orleans.Persistence.Redis.Serialization
{
	public class OrleansSerializer : ISerializer
	{
		private readonly SerializationManager _serializationManager;

		public OrleansSerializer(SerializationManager serializationManager)
		{
			_serializationManager = serializationManager;
		}

		public byte[] Serialize(object raw, Type type)
			=> _serializationManager.SerializeToByteArray(raw);

		public object Deserialize(byte[] serializedData, Type type)
			=> _serializationManager.Deserialize(type, new BinaryTokenStreamReader(serializedData));
	}
}