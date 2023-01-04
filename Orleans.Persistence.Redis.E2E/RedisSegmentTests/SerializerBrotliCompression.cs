using Orleans.Serialization;
using System;
using System.IO;
using System.IO.Compression;

namespace Orleans.Persistence.Redis.E2E.RedisSegmentTests;

public class SerializerBrotliCompression : Serialization.OrleansSerializer
{
	public SerializerBrotliCompression(SerializationManager serializationManager) : base(serializationManager)
	{
	}

	public override object Deserialize(byte[] serializedData, Type type)
		=> base.Deserialize(Decompress(serializedData), type);

	public override byte[] Serialize(object raw, Type type)
		=> Compress(base.Serialize(raw, type));

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
		using var brotliStream = new BrotliStream(memoryStream, CompressionLevel.Optimal);
		brotliStream.Write(bytes, 0, bytes.Length);
		return memoryStream.ToArray();
	}
}