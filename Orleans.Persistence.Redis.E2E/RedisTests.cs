using Orleans.Streams;
using StackExchange.Redis;
using System.Threading.Tasks;
using Xunit;
using static StackExchange.Redis.ConnectionMultiplexer;

namespace Orleans.Persistence.Redis.E2E
{
	public class RedisTests : TestBase
	{
		public RedisTests()
		{
			Initialize(3);
		}

		[Fact]
		public async Task ActivateGrainWithNoState()
		{
			await FlushDb();
			var state = await Cluster.GrainFactory.GetGrain<ITestGrain>("a-key-with-no-state").GetTheState();
			Assert.Equal(MockState.Empty, state);
		}

		[Fact]
		public async Task ActivateGrainWithState()
		{
			var grain = Cluster.GrainFactory.GetGrain<ITestGrain>("a-key-with-init-state");
			var mock = MockState.Generate();

			var done = new TaskCompletionSource<bool>();

			var provider = Cluster.Client.GetStreamProvider("TestStream");
			var stream = provider.GetStream<string>(StreamGuid, "deactivate-notifications");
			await stream.SubscribeAsync((message, seq) =>
			{
				done.SetResult(true);
				return Task.CompletedTask;
			});

			await grain.SaveMe(mock);
			await grain.Deactivate();

			await done.Task;

			var state = await grain.GetTheState();
			Assert.Equal(mock, state);
		}

		[Fact]
		public async Task ClearState()
		{
			var grain = Cluster.GrainFactory.GetGrain<ITestGrain>("a-key-for-clearing-state");
			var mock = MockState.Generate();

			await grain.SaveMe(mock);
			var state = await grain.GetTheState();
			Assert.Equal(mock, state);

			await grain.DeleteState();
			state = await grain.GetTheState();
			Assert.Equal(MockState.Empty, state);

			var done = new TaskCompletionSource<bool>();

			var provider = Cluster.Client.GetStreamProvider("TestStream");
			var stream = provider.GetStream<string>(StreamGuid, "deactivate-notifications");
			await stream.SubscribeAsync((message, seq) =>
			{
				done.SetResult(true);
				return Task.CompletedTask;
			});

			await grain.Deactivate();

			await done.Task;

			state = await grain.GetTheState();
			Assert.Equal(MockState.Empty, state);
		}

		[Fact]
		public async Task WriteNullToState()
		{
			var grain = Cluster.GrainFactory.GetGrain<ITestGrain>("a-key-for-writing-null");
			await grain.WriteNullToState();

			var state = await grain.GetTheState();
			Assert.Null(state);
		}

		[Fact]
		public async Task TestSecondProvider()
		{
			var grain = Cluster.GrainFactory.GetGrain<ITestGrain2>("a-key-for-the second-provider");
			var mock = MockState.Generate();

			await grain.SaveMe(mock);

			var state = await grain.GetTheState();
			Assert.Equal(mock, state);
		}

		private static async Task FlushDb()
		{
			using (var connection = await ConnectAsync(new ConfigurationOptions
			{
				EndPoints = { "localhost" },
				AllowAdmin = true
			}))
			{
				var server = connection.GetServer("localhost:6379");
				await server.FlushAllDatabasesAsync();
			}
		}
	}
}
