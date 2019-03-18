using Orleans;
using Orleans.Providers;
using System.Threading.Tasks;

namespace TestGrains
{
	[StorageProvider(ProviderName = "Test")]
	public class TestGrain : Grain<TestModel>, ITestGrain
	{
		public async Task<string> GetThePhrase()
		{
			const string phrase = "First message from the TestGrain. Now write Something and see it sent through kafka to the grains. (Will be printed in the Silo console window ;))";

			State = new TestModel
			{
				Greeting = phrase
			};

			await WriteStateAsync();

			return phrase;
		}

		public override async Task OnActivateAsync()
		{

		}
	}
}