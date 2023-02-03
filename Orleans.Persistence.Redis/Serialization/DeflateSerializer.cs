using Orleans.Serialization;
using System;
using System.IO;
using System.IO.Compression;

namespace Orleans.Persistence.Redis.Serialization;
public class DeflateSerializer : Serialization.OrleansSerializer
{
	public DeflateSerializer(SerializationManager serializationManager) : base(serializationManager)
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
		using (var decompressStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
		{
			decompressStream.CopyTo(outputStream);
		}
		return outputStream.ToArray();
	}

	private static byte[] Compress(byte[] bytes)
	{
		using var memoryStream = new MemoryStream();
		using (var compressStream = new DeflateStream(memoryStream, CompressionMode.Compress))
		{
			compressStream.Write(bytes, 0, bytes.Length);
		}
		return memoryStream.ToArray();
	}
}