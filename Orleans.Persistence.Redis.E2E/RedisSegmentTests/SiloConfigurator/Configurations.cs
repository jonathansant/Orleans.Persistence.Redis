using Microsoft.Extensions.Configuration;
using Orleans.Hosting;
using Orleans.TestingHost;
using System.Collections.Generic;

namespace Orleans.Persistence.Redis.E2E.RedisSegmentTests.SiloConfigurator
{
	public class SiloBuilderConfiguratorOrlensSerializer : ISiloBuilderConfigurator
	{
		public void Configure(ISiloHostBuilder hostBuilder)
			=> hostBuilder
				.ConfigureApplicationParts(parts =>
					parts.AddApplicationPart(typeof(ITestGrainSegments).Assembly).WithReferences())
				.AddRedisGrainStorage("TestingProvider")
				.Build(builder => builder.Configure(opts =>
				{
					opts.Servers = new List<string> { "localhost" };
					opts.ClientName = "testing";
					opts.ThrowExceptionOnInconsistentETag = false;
				}))
		;
	}
	public class SiloBuilderConfiguratorOrlensSerializerCompressed : ISiloBuilderConfigurator
	{
		public void Configure(ISiloHostBuilder hostBuilder)
			=> hostBuilder
				.ConfigureApplicationParts(parts =>
					parts.AddApplicationPart(typeof(ITestGrainSegments).Assembly).WithReferences())
				.AddRedisGrainStorage("TestingProvider")
				.AddDefaultRedisBrotliSerializer()
				.Build(builder => builder.Configure(opts =>
				{
					opts.Servers = new List<string> { "localhost" };
					opts.ClientName = "testing";
					opts.ThrowExceptionOnInconsistentETag = false;
				}))
		;
	}
	public class SiloBuilderConfiguratorOrlensSerializerSegmented : ISiloBuilderConfigurator
	{
		public void Configure(ISiloHostBuilder hostBuilder)
			=> hostBuilder
				.ConfigureApplicationParts(parts =>
					parts.AddApplicationPart(typeof(ITestGrainSegments).Assembly).WithReferences())
				.AddRedisGrainStorage("TestingProvider")
				.Build(builder => builder.Configure(opts =>
				{
					opts.Servers = new List<string> { "localhost" };
					opts.ClientName = "testing";
					opts.ThrowExceptionOnInconsistentETag = false;
					opts.SegmentSize = 1024 * 1024;
				}))
		;
	}
	public class SiloBuilderConfiguratorOrlensSerializerCompressedSegmented : ISiloBuilderConfigurator
	{
		public void Configure(ISiloHostBuilder hostBuilder)
			=> hostBuilder
				.ConfigureApplicationParts(parts =>
					parts.AddApplicationPart(typeof(ITestGrainSegments).Assembly).WithReferences())
				.AddRedisGrainStorage("TestingProvider")
				.AddDefaultRedisBrotliSerializer()
				.Build(builder => builder.Configure(opts =>
				{
					opts.Servers = new List<string> { "localhost" };
					opts.ClientName = "testing";
					opts.ThrowExceptionOnInconsistentETag = false;
					opts.SegmentSize = 1000;
				}))
		;
	}
	public class SiloBuilderConfiguratorHumanSerializer : ISiloBuilderConfigurator
	{
		public void Configure(ISiloHostBuilder hostBuilder)
			=> hostBuilder
				.ConfigureApplicationParts(parts =>
					parts.AddApplicationPart(typeof(ITestGrainSegments).Assembly).WithReferences())
				.AddRedisGrainStorage("TestingProvider")
				.Build(builder => builder.Configure(opts =>
				{
					opts.Servers = new List<string> { "localhost" };
					opts.ClientName = "testing";
					opts.ThrowExceptionOnInconsistentETag = false;
					opts.HumanReadableSerialization = true;
				}))
		;
	}

	public class SiloBuilderConfiguratorHumanSerializerSegmented : ISiloBuilderConfigurator
	{
		public void Configure(ISiloHostBuilder hostBuilder)
			=> hostBuilder
				.ConfigureApplicationParts(parts =>
					parts.AddApplicationPart(typeof(ITestGrainSegments).Assembly).WithReferences())
				.AddRedisGrainStorage("TestingProvider")
				.Build(builder => builder.Configure(opts =>
				{
					opts.Servers = new List<string> { "localhost" };
					opts.ClientName = "testing";
					opts.ThrowExceptionOnInconsistentETag = false;
					opts.SegmentSize = 1024 * 1024;
					opts.HumanReadableSerialization = true;
				}))
		;
	}

	public class ClientBuilderConfigurator : IClientBuilderConfigurator
	{
		public virtual void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
			=> clientBuilder
				.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ITestGrain).Assembly).WithReferences())
				.AddSimpleMessageStreamProvider("TestStream")
		;
	}
}
