using Orleans.Serialization;

namespace Orleans.Persistence.Redis.Serialization
{
	public class OrleansSerializer : ISerializer
	{
		private readonly Serializer _serializer;

		public OrleansSerializer(Serializer serializer)
		{
			_serializer = serializer;
		}

		public virtual byte[] Serialize(object raw)
			=> _serializer.SerializeToArray(raw);

		public virtual object Deserialize<T>(byte[] serializedData)
			=> _serializer.Deserialize<T>(serializedData);
	}
}