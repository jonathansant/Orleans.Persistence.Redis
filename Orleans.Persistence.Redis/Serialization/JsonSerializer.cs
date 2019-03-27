using Newtonsoft.Json;
using System;

namespace Orleans.Persistence.Redis.Serialization
{
	public class JsonSerializer : IHumanReadableSerializer
	{
		private readonly JsonSerializerSettings _settings;

		public JsonSerializer(JsonSerializerSettings settings)
		{
			_settings = settings;
		}

		public string Serialize(object raw)
			=> JsonConvert.SerializeObject(raw, _settings);

		public object Deserialize(string serializedData, Type type)
			=> JsonConvert.DeserializeObject(serializedData, type, _settings);
	}
}