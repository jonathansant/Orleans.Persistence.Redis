using Orleans.Serialization;
using System;

namespace Orleans.Persistence.Redis.Serialization
{
	public class OrleansSerializer : ISerializer
	{
		private readonly Serializer _serializationManager;

		public OrleansSerializer(Serializer serializationManager)
		{
			_serializationManager = serializationManager;
		}

		public virtual byte[] Serialize(object raw)
			=> _serializationManager.SerializeToArray(raw);

		public virtual object Deserialize<T>(byte[] serializedData)
			=> _serializationManager.Deserialize<T>(serializedData);
	}
}