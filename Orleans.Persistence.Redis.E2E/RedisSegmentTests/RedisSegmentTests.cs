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
	public class RedisOrleansSerializerTest : RedisSegmentTests<SiloConfigurator.SiloBuilderConfiguratorOrleansSerializer>
	{
		public RedisOrleansSerializerTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Test()
		{
			await PerformTest("SerializerTest");
		}
	}

	public class RedisOrleansSerializerCompressedTest : RedisSegmentTests<SiloConfigurator.SiloBuilderConfiguratorOrleansSerializerCompressed>
	{
		public RedisOrleansSerializerCompressedTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Test()
		{
			await PerformTest("SerializerCompressedTest");
		}
	}

	public class RedisOrleansSerializerDeflateCompressTest : RedisSegmentTests<SiloConfigurator.SiloBuilderConfiguratorOrleansSerializerDeflateCompression>
	{
		public RedisOrleansSerializerDeflateCompressTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Test()
		{
			await PerformTest("SerializerCompressDeflateTest");
		}
	}

	public class RedisOrleansSerializerSegmentedTest : RedisSegmentTests<SiloConfigurator.SiloBuilderConfiguratorOrleansSerializerSegmented>
	{
		public RedisOrleansSerializerSegmentedTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Test()
		{
			await PerformTest("OrleansSerializerSegmentedTest");
		}
		[Fact]
		public async Task TestSmallDataSet()
		{
			await PerformTest("OrleansSerializerSegmentedTestSDS", 1);
		}
	}

	public class RedisOrleansSerializerCompressedSegmentedTest : RedisSegmentTests<SiloConfigurator.SiloBuilderConfiguratorOrleansSerializerCompressedSegmented>
	{
		public RedisOrleansSerializerCompressedSegmentedTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Test()
		{
			await PerformTest("SerializerCompressedSegmentedTest");
		}
	}

	public class SiloBuilderConfiguratorOrleansSerializerDeflateCompressionSegmentedTest : RedisSegmentTests<SiloConfigurator.SiloBuilderConfiguratorOrleansSerializerDeflateCompressionSegmented>
	{
		public SiloBuilderConfiguratorOrleansSerializerDeflateCompressionSegmentedTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Test()
		{
			await PerformTest("SerializerCompressedDeflateSegmentedTest");
		}
	}

	public class RedisHumanSerializerTest : RedisSegmentTests<SiloConfigurator.SiloBuilderConfiguratorHumanSerializer>
	{
		public RedisHumanSerializerTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Test()
		{
			await PerformTest("HumanSerializerTest");
		}
	}

	public class RedisHumanSerializerSegmentedTest : RedisSegmentTests<SiloConfigurator.SiloBuilderConfiguratorHumanSerializerSegmented>
	{
		public RedisHumanSerializerSegmentedTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Test()
		{
			await PerformTest("HumanSerializerSegmentedTest");
		}
		[Fact]
		public async Task TestSmallDataSet()
		{
			await PerformTest("HumanSerializerSegmentedTestSDS", 1);
		}
	}

	public class CompressedTest : RedisSegmentTests<SiloConfigurator.SiloBuilderConfiguratorHumanSerializerCompressed>
	{
		public CompressedTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Test()
		{
			await PerformTest("HumanSerializerCompressedTest");
		}
	}

	public class CompressedTestWithSegments : RedisSegmentTests<SiloConfigurator.SiloBuilderConfiguratorHumanSerializerCompressedWithSegments>
	{
		public CompressedTestWithSegments(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Test()
		{
			await PerformTest("HumanSerializerCompressedTestWithSegments");
		}
	}

	public class RedisSegmentTests<T> : TestBase<T, SiloConfigurator.ClientBuilderConfigurator> where T : ISiloBuilderConfigurator, new()
	{
		private readonly ITestOutputHelper _output;

		public RedisSegmentTests(ITestOutputHelper output)
		{
			_output = output;
			Initialize(3);
		}

		private async Task<string> Arrange(int n = 100)
		{
			await FlushDb();
			// arrange
			var dataGenerated = await GenerateData(n);
			Assert.NotNull(dataGenerated);
			return dataGenerated;
		}

		public async Task PerformTest(string name, int n = 100)
		{
			try
			{
				var dataGenerated = await Arrange(n);
				var original = new HashSet<string>();

				var grain = Cluster.GrainFactory.GetGrain<ITestGrainSegments>($"segmentTest_{name}");

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
