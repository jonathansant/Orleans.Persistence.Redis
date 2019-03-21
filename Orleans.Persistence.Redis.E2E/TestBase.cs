using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Orleans.Hosting;
using Orleans.Persistence.Redis.Config;
using Orleans.Persistence.Redis.Serialization;
using Orleans.TestingHost;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.Persistence.Redis.E2E
{
	public class TestBase : IAsyncLifetime
	{
		private short _noOfSilos;

		public static Guid StreamGuid { get; } = Guid.Parse("20b5d2a3-9cce-4fec-8ee7-9869e88f77a7");

		protected TestCluster Cluster { get; private set; }

		protected void Initialize(short noOfSilos)
			=> _noOfSilos = noOfSilos;

		protected void ShutDown()
			=> Cluster?.StopAllSilos();

		public Task InitializeAsync()
		{
			var builder = new TestClusterBuilder(_noOfSilos);

			builder.AddSiloBuilderConfigurator<SiloBuilderConfigurator>();
			builder.AddClientBuilderConfigurator<ClientBuilderConfigurator>();

			Cluster = builder.Build();
			Cluster.Deploy();

			return Task.CompletedTask;
		}

		public Task DisposeAsync()
		{
			ShutDown();
			return Task.CompletedTask;
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
//						opts.PlainTextSerialization = true;
					})
				)
				.UseRedisMessagePackSerializer("TestingProvider")
				.AddRedisGrainStorage("TestingProvider2",
					builder => builder.Configure(opts => opts.Servers = new List<string> { "127.0.0.1" })
				)
				.UseRedisJsonSerializer("TestingProvider2", new JsonSerializerSettings())
				.AddSimpleMessageStreamProvider("TestStream")
				.AddMemoryGrainStorage("PubSubStore")
		;
	}
}
