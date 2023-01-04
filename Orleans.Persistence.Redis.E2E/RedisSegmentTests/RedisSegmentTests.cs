using Microsoft.Extensions.Configuration;
using Orleans.Hosting;
using Orleans.TestingHost;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Orleans.Persistence.Redis.E2E.RedisSegmentTests
{
	public class RedisSegmentTests : TestBase<RedisSegmentTests.SiloBuilderConfigurator, RedisSegmentTests.ClientBuilderConfigurator>
	{
		private readonly ITestOutputHelper _output;

		public RedisSegmentTests(ITestOutputHelper output)
		{
			_output = output;
			Initialize(3);
		}


		private async Task<string> Arrange()
		{
			await FlushDb();

			// arrange
			var dataGenerated = await GenerateData(10 * 10);
			Assert.NotNull(dataGenerated);
			return dataGenerated;
		}

		[Fact]
		public async Task SegmentTest1()
		{
			try
			{
				var dataGenerated = await Arrange();
				var original = new HashSet<string>();

				var grain = Cluster.GrainFactory.GetGrain<ITestGrainSegments>("segmentTest");

				original.Add(dataGenerated);

				// Act
				await grain.AddData(new BigData()
				{
					Data = dataGenerated
				});

				var watch = new Stopwatch();

				watch.Start();
				var read = await grain.GetData(true);
				watch.Stop();

				_output.WriteLine("Duration {0} ms", watch.Elapsed.TotalMilliseconds);

				// Assert
				Assert.Contains(original, x => x.Contains(read.Data));
			}
			catch (Exception e)
			{
				_output.WriteLine("Exception:  {0}", e.Message);
				throw;
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
						parts.AddApplicationPart(typeof(ITestGrainSegments).Assembly).WithReferences())
					.AddRedisGrainStorage("TestingProvider")
					.AddRedisDefaultHumanReadableSerializer()
					.Build(builder => builder.Configure(opts =>
					{
						opts.Servers = new List<string> { "localhost" };
						opts.ClientName = "testing";
						opts.HumanReadableSerialization = true;
						opts.ThrowExceptionOnInconsistentETag = false;
						opts.DeleteOldSegments = false;
						opts.SegmentSize = 1024 * 1024;
					}))
				;
		}
		private static async Task<string> GenerateData(int n) // 100kb
		{
			const string filename = @"RedisSegmentTests\data_100kb.txt";
			Assert.True(File.Exists(filename));

			var temp = await File.ReadAllTextAsync(filename);
			var sb = new StringBuilder();
			for (var i = 0; i < n; i++)
			{
				sb.Append(temp);
			}
			return sb.ToString();
		}
	}
}
