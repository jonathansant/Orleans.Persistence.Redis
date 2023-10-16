using Humanizer;
using Microsoft.Extensions.Configuration;
using Orleans.Hosting;
using Orleans.Persistence.Redis.Serialization;
using Orleans.TestingHost;
using System.Collections.Generic;

namespace Orleans.Persistence.Redis.E2E.RedisSegmentTests.SiloConfigurator
{
	public class SiloBuilderConfiguratorOrleansSerializer : ISiloConfigurator
	{
		public void Configure(ISiloBuilder hostBuilder)
			=> hostBuilder
				.AddRedisGrainStorage("TestingProvider")
				.Build(builder => builder.Configure(opts =>
				{
					opts.Servers = new List<string> { "localhost" };
					opts.ClientName = "testing";
					opts.ThrowExceptionOnInconsistentETag = false;
				}))
		;
	}
	public class SiloBuilderConfiguratorOrleansSerializerCompressed : ISiloConfigurator
	{
		public void Configure(ISiloBuilder hostBuilder)
			=> hostBuilder
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
	public class SiloBuilderConfiguratorOrleansSerializerDeflateCompression : ISiloConfigurator
	{
		public void Configure(ISiloBuilder hostBuilder)
			=> hostBuilder
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

	public class SiloBuilderConfiguratorOrleansSerializerSegmented : ISiloConfigurator
	{
		public void Configure(ISiloBuilder hostBuilder)
			=> hostBuilder
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

	public class SiloBuilderConfiguratorOrleansSerializerCompressedSegmented : ISiloConfigurator
	{
		public void Configure(ISiloBuilder hostBuilder)
			=> hostBuilder
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

	public class SiloBuilderConfiguratorOrleansSerializerDeflateCompressionSegmented : ISiloConfigurator
	{
		public void Configure(ISiloBuilder hostBuilder)
			=> hostBuilder
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

	public class SiloBuilderConfiguratorHumanSerializer : ISiloConfigurator
	{
		public void Configure(ISiloBuilder hostBuilder)
			=> hostBuilder
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

	public class SiloBuilderConfiguratorHumanSerializerSegmented : ISiloConfigurator
	{
		public void Configure(ISiloBuilder hostBuilder)
			=> hostBuilder
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

	public class SiloBuilderConfiguratorHumanSerializerCompressed : ISiloConfigurator
	{
		public void Configure(ISiloBuilder hostBuilder)
			=> hostBuilder
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

	public class SiloBuilderConfiguratorHumanSerializerCompressedWithSegments : ISiloConfigurator
	{
		public void Configure(ISiloBuilder hostBuilder)
			=> hostBuilder
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
				.AddMemoryStreams("TestStream")
		;
	}
}
