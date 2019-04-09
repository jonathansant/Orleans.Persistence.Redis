using System;

namespace Orleans.Persistence.Redis.Serialization
{
	public interface IHumanReadableSerializer
	{
		string Serialize(IGrainState raw);
		IGrainState Deserialize(string serializedData, Type type);
	}

	public interface ISerializer
	{
		byte[] Serialize(IGrainState raw, Type type);
		IGrainState Deserialize(byte[] serializedData, Type type);
	}
}
