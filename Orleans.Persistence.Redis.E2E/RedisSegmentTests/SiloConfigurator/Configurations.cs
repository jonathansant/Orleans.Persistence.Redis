using Humanizer;
using Microsoft.Extensions.Configuration;
using Orleans.Hosting;
using Orleans.Persistence.Redis.Serialization;
using Orleans.TestingHost;
using System.Collections.Generic;

namespace Orleans.Persistence.Redis.E2E.RedisSegmentTests.SiloConfigurator
{
	public class SiloBuilderConfiguratorOrleansSerializer : ISiloBuilderConfigurator
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
	public class SiloBuilderConfiguratorOrleansSerializerCompressed : ISiloBuilderConfigurator
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
	public class SiloBuilderConfiguratorOrleansSerializerDeflateCompression : ISiloBuilderConfigurator
	{
		public void Configure(ISiloHostBuilder hostBuilder)
			=> hostBuilder
				.ConfigureApplicationParts(parts =>
					parts.AddApplicationPart(typeof(ITestGrainSegments).Assembly).WithReferences())
				.AddRedisGrainStorage("TestingProvider")
				.AddRedisSerializer<DeflateSerializer>()
				.Build(builder => builder.Configure(opts =>
				{
					opts.Servers = new List<string> { "localhost" };
					opts.ClientName = "testing";
					opts.ThrowExceptionOnInconsistentETag = false;
				}))
		;
	}

	public class SiloBuilderConfiguratorOrleansSerializerSegmented : ISiloBuilderConfigurator
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
					opts.SegmentSize = (int)1.Megabytes().Bytes;
				}))
		;
	}

	public class SiloBuilderConfiguratorOrleansSerializerCompressedSegmented : ISiloBuilderConfigurator
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
					opts.SegmentSize = (int)1.Kilobytes().Bytes;
				}))
		;
	}

	public class SiloBuilderConfiguratorOrleansSerializerDeflateCompressionSegmented : ISiloBuilderConfigurator
	{
		public void Configure(ISiloHostBuilder hostBuilder)
			=> hostBuilder
				.ConfigureApplicationParts(parts =>
					parts.AddApplicationPart(typeof(ITestGrainSegments).Assembly).WithReferences())
				.AddRedisGrainStorage("TestingProvider")
				.AddRedisSerializer<DeflateSerializer>()
				.Build(builder => builder.Configure(opts =>
				{
					opts.Servers = new List<string> { "localhost" };
					opts.ClientName = "testing";
					opts.ThrowExceptionOnInconsistentETag = false;
					opts.SegmentSize = (int)1.Kilobytes().Bytes;
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
					opts.SegmentSize = (int)1.Megabytes().Bytes;
					opts.HumanReadableSerialization = true;
				}))
		;
	}

	public class SiloBuilderConfiguratorHumanSerializerCompressed : ISiloBuilderConfigurator
	{
		public void Configure(ISiloHostBuilder hostBuilder)
			=> hostBuilder
				.ConfigureApplicationParts(parts =>
					parts.AddApplicationPart(typeof(ITestGrainSegments).Assembly).WithReferences())
				.AddRedisGrainStorage("TestingProvider")
				.AddCompression<Compression.RawDeflateCompression>()
				//.AddCompression<Compression.BrotliCompression>()
				//.AddCompression<Compression.GZipCompression>()

				.Build(builder => builder.Configure(opts =>
				{
					opts.Servers = new List<string> { "localhost" };
					opts.ClientName = "testing";
					opts.ThrowExceptionOnInconsistentETag = false;
					opts.HumanReadableSerialization = true;
				}));
	}

	public class SiloBuilderConfiguratorHumanSerializerCompressedWithSegments : ISiloBuilderConfigurator
	{
		public void Configure(ISiloHostBuilder hostBuilder)
			=> hostBuilder
				.ConfigureApplicationParts(parts =>
					parts.AddApplicationPart(typeof(ITestGrainSegments).Assembly).WithReferences())
				.AddRedisGrainStorage("TestingProvider")
				.AddCompression<Compression.RawDeflateCompression>()
				//.AddCompression<Compression.BrotliCompression>()
				//.AddCompression<Compression.GZipCompression>()

				.Build(builder => builder.Configure(opts =>
				{
					opts.Servers = new List<string> { "localhost" };
					opts.ClientName = "testing";
					opts.ThrowExceptionOnInconsistentETag = false;
					opts.HumanReadableSerialization = true;
					opts.SegmentSize = (int)(100.Kilobytes().Bytes);
				}));
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
