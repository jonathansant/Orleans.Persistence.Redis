using System;

namespace Orleans.Persistence.Redis.Serialization
{
	public interface IHumanReadableSerializer
	{
		string Serialize(object raw);
		object Deserialize(string serializedData, Type type);
	}

	public interface ISerializer
	{
		byte[] Serialize(object raw);
		object Deserialize(byte[] serializedData, Type type);
	}
}
