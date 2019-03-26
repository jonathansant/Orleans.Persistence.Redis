using Microsoft.Extensions.Configuration;
using Orleans.Hosting;
using Orleans.Persistence.Redis.Config;
using Orleans.Persistence.Redis.Serialization;
using Orleans.Streams;
using Orleans.TestingHost;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.Persistence.Redis.E2E
{
	public class MessagePackTests : TestBase<MessagePackTests.SiloBuilderConfigurator, MessagePackTests.ClientBuilderConfigurator>
	{
		public MessagePackTests()
		{
			Initialize(3);
		}

		[Fact]
		public async Task ActivateGrainWithState()
		{
			var grain = Cluster.GrainFactory.GetGrain<ITestGrain>("a-key-with-init-state-mp-serialized");
			var mock = MockState.Generate();

			var done = new TaskCompletionSource<bool>();

			var provider = Cluster.Client.GetStreamProvider("TestStream");
			var stream = provider.GetStream<string>(Consts.StreamGuid, "deactivate-notifications");
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

		public class ClientBuilderConfigurator : IClientBuilderConfigurator
		{
			public virtual void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
				=> clientBuilder
					.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ITestGrain).Assembly).WithReferences())
					.AddSimpleMessageStreamProvider("TestStream")
			;
		}

		public class SiloBuilderConfigurator : ISiloBuilderConfigurator
		{
			public void Configure(ISiloHostBuilder hostBuilder)
				=> hostBuilder
					.ConfigureApplicationParts(parts =>
						parts.AddApplicationPart(typeof(ITestGrain).Assembly).WithReferences())
					.AddRedisGrainStorage("TestingProvider",
						builder => builder.Configure(opts =>
						{
							opts.Servers = new List<string> { "localhost" };
							opts.ClientName = "testing";
						})
					)
					.AddRedisSerializer<MessagePackSerializer>("TestingProvider")
					.AddRedisDefaultHumanReadableSerializer("TestingProvider")
					.AddSimpleMessageStreamProvider("TestStream")
					.AddMemoryGrainStorage("PubSubStore")
			;
		}
	}
}
