using Orleans.Serialization;
using System.IO.Compression;

namespace Orleans.Persistence.Redis.Serialization
{
	public class BrotliSerializer : OrleansSerializer
	{
		public BrotliSerializer(Serializer serializer) : base(serializer)
		{
		}

		public override object Deserialize<T>(byte[] serializedData)
			=> base.Deserialize<T>(Decompress(serializedData));

		public override byte[] Serialize(object raw)
			=> Compress(base.Serialize(raw));

		private static byte[] Decompress(byte[] bytes)
		{
			using var memoryStream = new MemoryStream(bytes);
			using var outputStream = new MemoryStream();
			using (var decompressStream = new BrotliStream(memoryStream, CompressionMode.Decompress))
			{
				decompressStream.CopyTo(outputStream);
			}

			return outputStream.ToArray();
		}

		private static byte[] Compress(byte[] bytes)
		{
			using var memoryStream = new MemoryStream();
			using (var brotliStream = new BrotliStream(memoryStream, CompressionMode.Compress))
			{
				brotliStream.Write(bytes, 0, bytes.Length);
			}

			return memoryStream.ToArray();
		}
	}
}