namespace Orleans.Persistence.Redis.Compression
{
	public interface ICompression
	{
		byte[] Decompress(byte[] buffer);
		byte[] Compress(byte[] buffer);
	}
}
