using Orleans.Persistence.Redis.Compression;
using System.Text;
using Xunit;

namespace DeflateCompressionTest
{
	public class DeflateCompressionUnitTest
	{
		[Fact]
		public void DeflateCompressionTest1()
		{
			const string test = "hello world";
			var deflateCompression = new DeflateCompression();

			var compressed = deflateCompression.Compress(Encoding.UTF8.GetBytes(test));
			var decompressed = deflateCompression.Decompress(compressed);

			Assert.NotNull(decompressed);
			Assert.Equal(test, Encoding.UTF8.GetString(decompressed));
		}
	}
}