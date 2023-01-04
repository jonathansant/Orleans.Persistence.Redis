using Orleans.Providers;
using System.Threading.Tasks;

namespace Orleans.Persistence.Redis.E2E.RedisSegmentTests
{
	public class BigData
	{
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
		public override async Task OnActivateAsync()
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
