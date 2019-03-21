using Orleans.Serialization;
using System;
using System.Text;

namespace Orleans.Persistence.Redis.Serialization
{
	public class DefaultSerializer : ISerializer
	{
		private readonly SerializationManager _serializationManager;

		public DefaultSerializer(SerializationManager serializationManager)
		{
			_serializationManager = serializationManager;
		}

		public byte[] Serialize(object raw)
			=> _serializationManager.SerializeToByteArray(raw);

		public string SerializeToString(object raw)
		{
			var bytes = _serializationManager.SerializeToByteArray(raw);
			return Encoding.UTF8.GetString(bytes);
		}

		public object Deserialize(byte[] serializedData, Type type)
			=> _serializationManager.Deserialize(type, new BinaryTokenStreamReader(serializedData));

		public object Deserialize(string serializedData, Type type)
		{
			var stream = new BinaryTokenStreamReader(Encoding.UTF8.GetBytes(serializedData));
			return _serializationManager.Deserialize(stream);
		}
	}
}