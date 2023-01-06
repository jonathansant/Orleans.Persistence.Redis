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
	public class RedisOrleansSerializerTest : RedisSegmentTests<SiloConfigurator.SiloBuilderConfiguratorOrlensSerializer>
	{
		public RedisOrleansSerializerTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Test()
		{
			await PerformTest();
		}
	}

	public class RedisOrleansSerializerCompressedTest : RedisSegmentTests<SiloConfigurator.SiloBuilderConfiguratorOrlensSerializerCompressed>
	{
		public RedisOrleansSerializerCompressedTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Test()
		{
			await PerformTest();
		}
	}

	public class RedisOrleansSerializerSegmentedTest : RedisSegmentTests<SiloConfigurator.SiloBuilderConfiguratorOrlensSerializerSegmented>
	{
		public RedisOrleansSerializerSegmentedTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Test()
		{
			await PerformTest();
		}
	}

	public class RedisOrleansSerializerCompressedSegmentedTest : RedisSegmentTests<SiloConfigurator.SiloBuilderConfiguratorOrlensSerializerCompressedSegmented>
	{
		public RedisOrleansSerializerCompressedSegmentedTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Test()
		{
			await PerformTest();
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
			await PerformTest();
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
			await PerformTest();
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

		private async Task<string> Arrange()
		{
			await FlushDb();
			// arrange
			var dataGenerated = await GenerateData(10 * 10);
			Assert.NotNull(dataGenerated);
			return dataGenerated;
		}

		public async Task PerformTest()
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
