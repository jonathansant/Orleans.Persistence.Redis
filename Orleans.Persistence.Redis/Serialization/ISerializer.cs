namespace Orleans.Persistence.Redis.Serialization
{
	public interface IHumanReadableSerializer
	{
		string Serialize(object raw, Type type);
		object Deserialize(string serializedData, Type type);
		string Serialize<T>(T raw);
		T Deserialize<T>(string serializedData);
	}

	public interface ISerializer
	{
		byte[] Serialize(object raw);
		object Deserialize<T>(byte[] serializedData);
	}
}
