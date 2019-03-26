using Microsoft.Extensions.Configuration;
using Orleans.Hosting;
using Orleans.Persistence.Redis.Config;
using Orleans.TestingHost;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static StackExchange.Redis.ConnectionMultiplexer;

namespace Orleans.Persistence.Redis.E2E
{
	public class TestBase<TSilo, TClient> : IAsyncLifetime
		where TSilo : ISiloBuilderConfigurator, new()
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
						opts.KeyPrefix = "prefix";
					})
				)
				.AddRedisDefaultSerializer("TestingProvider")
				.AddRedisDefaultHumanReadableSerializer("TestingProvider")
				.AddRedisGrainStorage("TestingProvider2",
					builder => builder.Configure(opts =>
					{
						opts.Servers = new List<string> { "127.0.0.1" };
						opts.ClientName = "testing";
						opts.KeyPrefix = "prefix";
					})
				)
				.AddRedisDefaultSerializer("TestingProvider2")
				.AddRedisDefaultHumanReadableSerializer("TestingProvider2")
				.AddSimpleMessageStreamProvider("TestStream")
				.AddMemoryGrainStorage("PubSubStore")
		;
	}
}
