using Orleans.Providers;

namespace Orleans.Persistence.Redis.E2E.RedisSegmentTests
{
	[GenerateSerializer]
	public class BigData
	{
		[Id(0)]
		public string Data { get; set; }
	}

	public interface ITestGrainSegments : IGrainWithStringKey
	{
		Task<BigData> GetData(bool bForceRead = false);
		Task AddData(BigData data);
	}


	[StorageProvider(ProviderName = "TestingProvider")]
	public class TestGrainSegments : Grain<BigData>, ITestGrainSegments
	{
		public override async Task OnActivateAsync(CancellationToken _)
		{
		}

		public async Task<BigData> GetData(bool bForceRead = false)
		{
			if (bForceRead)
				await ReadStateAsync();

			return State;
		}


		public async Task AddData(BigData data)
		{
			State = data;
			await WriteStateAsync();
		}
	}
}
