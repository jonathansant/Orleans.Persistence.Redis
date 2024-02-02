using Newtonsoft.Json;
using Orleans.Persistence.Redis.Compression;

namespace Orleans.Persistence.Redis.Serialization
{
	public class JsonSerializer : IHumanReadableSerializer
	{
		private readonly JsonSerializerSettings _settings;
		private readonly ICompression _compression;

		public JsonSerializer(
			JsonSerializerSettings settings,
			ICompression? compression = null
		)
		{
			_settings = settings;
			_compression = compression;
		}

		public string Serialize(object raw, Type type)
			=> JsonConvert.SerializeObject(raw, _settings);

		public object Deserialize(string serializedData, Type type)
			=> JsonConvert.DeserializeObject(serializedData, type, _settings);

		public string Serialize<T>(T raw)
			=> Serialize(raw, typeof(T));

		public T Deserialize<T>(string serializedData)
			=> (T)Deserialize(serializedData, typeof(T));
	}
}