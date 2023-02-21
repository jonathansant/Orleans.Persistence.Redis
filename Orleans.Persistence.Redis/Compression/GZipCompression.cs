using System.IO;
using System.IO.Compression;

namespace Orleans.Persistence.Redis.Compression
{
	public class GZipCompression : ICompression
	{
		public byte[] Decompress(byte[] buffer)
		{
			using var memoryStream = new MemoryStream(buffer);
			using var outputStream = new MemoryStream();
			using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
			{
				decompressStream.CopyTo(outputStream);
			}

			return outputStream.ToArray();
		}

		public byte[] Compress(byte[] buffer)
		{
			using var memoryStream = new MemoryStream();
			using (var compressStream = new GZipStream(memoryStream, CompressionMode.Compress))
			{
				compressStream.Write(buffer, 0, buffer.Length);
			}

			return memoryStream.ToArray();
		}
	}
}
