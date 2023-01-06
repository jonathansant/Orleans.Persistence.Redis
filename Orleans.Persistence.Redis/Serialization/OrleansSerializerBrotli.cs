using Brotli;
using Orleans.Serialization;
using System;
using System.IO;
using System.IO.Compression;

namespace Orleans.Persistence.Redis.Serialization;
public class OrleansSerializerBrotli : Serialization.OrleansSerializer
{
	public OrleansSerializerBrotli(SerializationManager serializationManager) : base(serializationManager)
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
		using (var brotliStream = new BrotliStream(memoryStream, CompressionMode.Compress))
		{
			brotliStream.Write(bytes, 0, bytes.Length);
		}
		return memoryStream.ToArray();
	}
}