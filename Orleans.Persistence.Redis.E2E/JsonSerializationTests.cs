using System;
using Microsoft.Extensions.Configuration;
using Orleans.Hosting;
using Orleans.Streams;
using Orleans.TestingHost;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.Persistence.Redis.E2E
{
	public class JsonTests : TestBase<JsonTests.SiloBuilderConfigurator, JsonTests.ClientBuilderConfigurator>
	{
		public JsonTests()
		{
			Initialize(3);
		}

		[Fact]
		public async Task ActivateGrainWithState()
		{
			var grain = Cluster.GrainFactory.GetGrain<ITestGrain>("a-key-with-init-state-json-serialized");
			var mock = MockState.Generate();

			var done = new TaskCompletionSource<bool>();

			var provider = Cluster.Client.GetStreamProvider("TestStream");
			var stream = provider.GetStream<string>("deactivate-notifications", Consts.StreamGuid);
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
					.AddMemoryStreams("TestStream")
			;
		}

		public class SiloBuilderConfigurator : ISiloConfigurator
		{
			public void Configure(ISiloBuilder hostBuilder)
				=> hostBuilder
					.AddRedisGrainStorage("TestingProvider")
					.AddRedisDefaultHumanReadableSerializer()
					.Build(builder => builder.Configure(opts =>
						{
							opts.Servers = new List<string> { "localhost" };
							opts.ClientName = "testing";
							opts.HumanReadableSerialization = true;
							opts.KeyPrefix = "prefix-json";
						})
					)
					.AddMemoryStreams("TestStream")
					.AddMemoryGrainStorage("PubSubStore")
			;
		}
	}

	public class JsonPubSubTests : TestBase<JsonPubSubTests.SiloBuilderConfigurator, JsonPubSubTests.ClientBuilderConfigurator>
	{
		public JsonPubSubTests()
		{
			Initialize(1);
		}

		[Fact]
		public async Task PubSub()
		{
			var grain = Cluster.GrainFactory.GetGrain<ITestStreamerGrain>("a-grain-that-will-pubsub-rendevos-streams");
			await grain.Invoke();

			var provider = Cluster.Client.GetStreamProvider("TestStream");
			var stream = provider.GetStream<int>("multi-notifications", Consts.StreamGuid);

			for (var i = 0; i < 10; i++)
			{
				await stream.OnNextAsync(i);
				if (i == 1)
				{
					var silo = Cluster.Silos.First();
					await Cluster.RestartSiloAsync(silo);
					await Task.Delay(10000);
				}
			}
		}

		public class ClientBuilderConfigurator : IClientBuilderConfigurator
		{
			public virtual void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
				=> clientBuilder
					.AddMemoryStreams("TestStream")
			;
		}

		public class SiloBuilderConfigurator : ISiloConfigurator
		{
			public void Configure(ISiloBuilder hostBuilder)
				=> hostBuilder
					.AddRedisGrainStorage("TestingProvider")
					.AddRedisDefaultHumanReadableSerializer()
					.Build(builder => builder.Configure(opts =>
					{
						opts.Servers = new List<string> { "localhost" };
						opts.ClientName = "testing";
						opts.HumanReadableSerialization = true;
						opts.KeyPrefix = "prefix-json";
					}))
					.AddRedisGrainStorage("PubSubStore")
					.AddRedisDefaultHumanReadableSerializer()
					.Build(builder => builder.Configure(opts =>
					{
						opts.Servers = new List<string> { "localhost" };
						opts.ClientName = "testing-pubsub";
						opts.HumanReadableSerialization = true;
						opts.KeyPrefix = "prefix-json-pubsub";
					}))
					.AddMemoryStreams("TestStream")
			;
		}
	}
}
