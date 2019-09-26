using System;

namespace Orleans.Persistence.Redis.Serialization
{
	public interface IHumanReadableSerializer
	{
		string Serialize(object raw, Type type);
		object Deserialize(string serializedData, Type type);
	}

	public interface ISerializer
	{
		byte[] Serialize(object raw, Type type);
		object Deserialize(byte[] serializedData, Type type);
	}
}
