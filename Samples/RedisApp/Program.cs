using StackExchange.Redis;

namespace RedisApp
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var redis = ConnectionMultiplexer.Connect("localhost");
			var db = redis.GetDatabase(2);

			await db.StringAppendAsync("two", "yes");
			Console.WriteLine(await db.StringGetAsync("two"));

			Console.ReadKey();
		}
	}
}
