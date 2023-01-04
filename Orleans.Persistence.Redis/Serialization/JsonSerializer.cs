using Newtonsoft.Json;
using System;

namespace Orleans.Persistence.Redis.Serialization
{
	public class JsonSerializer : IHumanReadableSerializer
	{
		private readonly JsonSerializerSettings _settings;

		public JsonSerializer(
			JsonSerializerSettings settings
		)
		{
			_settings = settings;
		}

		public string Serialize(object raw, Type type)
			=> JsonConvert.SerializeObject(raw, _settings);

		public object Deserialize(string serializedData, Type type)
			=> JsonConvert.DeserializeObject(serializedData, type, _settings);

		public string Serialize<T>(T raw)
			=> JsonConvert.SerializeObject(raw);

		public T Deserialize<T>(string serializedData)
			=> JsonConvert.DeserializeObject<T>(serializedData);
	}
}