using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Orleans.Persistence.Redis.Compression
{
	public class DeflateCompression : ICompression
	{
		public byte[] Decompress(byte[] buffer)
		{
			var tempBuffer = new byte[buffer.Length - 6]; // 2 header + 4 checksum
			if (buffer[0] != 0x78 && buffer[0] != 0x9c)
				return null;

			Array.Copy(buffer, 2, tempBuffer, 0, tempBuffer.Length);

			using var memoryStream = new MemoryStream(tempBuffer);
			using var outputStream = new MemoryStream();
			using (var decompressStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
			{
				decompressStream.CopyTo(outputStream);
			}

			var decompressed = outputStream.ToArray();

			var checksum = CalculateChecksum(decompressed);

			return checksum.Where((t, i) => t != buffer[buffer.Length - 4 + i]).Any()
				? null
				: decompressed;
		}

		public byte[] Compress(byte[] buffer)
		{
			byte[] tempData;
			using (var memoryStream = new MemoryStream())
			{
				// header
				memoryStream.WriteByte(0x78);
				memoryStream.WriteByte(0x9C);
				using (var compressStream = new DeflateStream(memoryStream, CompressionMode.Compress))
					compressStream.Write(buffer, 0, buffer.Length);

				tempData = memoryStream.ToArray();
			}

			var n = tempData.Length;
			Array.Resize(ref tempData, n + 4);
			var checksum = CalculateChecksum(buffer);
			Array.Copy(checksum, 0, tempData, n, checksum.Length);
			return tempData;
		}

		private byte[] CalculateChecksum(byte[] buffer)
		{
			var a1 = 1;
			var a2 = 0;
			var checksum = new byte[4];

			foreach (var b in buffer)
			{
				a1 = (a1 + b) % 65521;
				a2 = (a2 + a1) % 65521;
			}

			checksum[0] = (byte)(a2 >> 8);
			checksum[1] = (byte)a2;
			checksum[2] = (byte)(a1 >> 8);
			checksum[3] = (byte)a1;

			return checksum;
		}
	}
}
