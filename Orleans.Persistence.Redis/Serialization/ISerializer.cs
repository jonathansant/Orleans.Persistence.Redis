using System;
using System.Threading.Tasks;

namespace Orleans.Persistence.Redis.Serialization
{
	public interface ISerializer
	{
		byte[] Serialize(object raw);
		string SerializeToString(object raw);
		object Deserialize(byte[] serializedData, Type type);
		object Deserialize(string serializedData, Type type);
	}
}
