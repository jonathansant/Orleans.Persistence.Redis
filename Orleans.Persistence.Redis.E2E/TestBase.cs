using Microsoft.Extensions.Configuration;
using Orleans.TestingHost;
using StackExchange.Redis;
using Xunit;
using static StackExchange.Redis.ConnectionMultiplexer;

namespace Orleans.Persistence.Redis.E2E
{
	public class TestBase<TSilo, TClient> : IAsyncLifetime
		where TSilo : ISiloConfigurator, new()
		where TClient : IClientBuilderConfigurator, new()
	{
		private short _noOfSilos;

		protected TestCluster Cluster { get; private set; }

		protected void Initialize(short noOfSilos)
			=> _noOfSilos = noOfSilos;

		protected void ShutDown()
			=> Cluster?.StopAllSilos();

		public Task InitializeAsync()
		{
			var builder = new TestClusterBuilder(_noOfSilos);

			builder.AddSiloBuilderConfigurator<TSilo>();
			builder.AddClientBuilderConfigurator<TClient>();

			Cluster = builder.Build();
			Cluster.Deploy();

			return Task.CompletedTask;
		}

		public Task DisposeAsync()
		{
			ShutDown();
			return Task.CompletedTask;
		}

		protected static async Task FlushDb()
		{
			await using var connection = await ConnectAsync(new ConfigurationOptions
			{
				EndPoints = { "localhost" },
				AllowAdmin = true
			});

			var server = connection.GetServer("localhost:6379");
			await server.FlushAllDatabasesAsync();
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
				.Build(builder => builder.Configure(opts =>
					{
						opts.Servers = new List<string> { "localhost" };
						opts.ClientName = "testing";
						opts.KeyPrefix = "prefix";
					})
				)
				.AddRedisGrainStorage("TestingProvider2")
				.Build(builder => builder.Configure(opts =>
				{
					opts.Servers = new List<string> { "localhost" };
					opts.ClientName = "testing";
					opts.KeyPrefix = "prefix";
				}))
				.AddMemoryStreams("TestStream")
				.AddMemoryGrainStorage("PubSubStore")
		;
	}
}
