﻿using System.IO;
using System.IO.Compression;

namespace Orleans.Persistence.Redis.Compression
{
	public class BrotliCompression : ICompression
	{
		public byte[] Decompress(byte[] bytes)
		{
			using var memoryStream = new MemoryStream(bytes);
			using var outputStream = new MemoryStream();
			using (var decompressStream = new BrotliStream(memoryStream, CompressionMode.Decompress))
			{
				decompressStream.CopyTo(outputStream);
			}

			return outputStream.ToArray();
		}

		public byte[] Compress(byte[] bytes)
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
