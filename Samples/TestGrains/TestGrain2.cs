using Orleans.Providers;

namespace TestGrains
{
	[StorageProvider(ProviderName = "Test")]
	public class TestGrain2 : Grain<object>, ITestGrain2
	{
		private readonly Random _rand = new Random();

		public async Task Save()
		{
			State = "test 1" + _rand.Next(1000);
			await WriteStateAsync();
		}

		public async Task Delete()
		{
			await ClearStateAsync();
		}

		public override Task OnActivateAsync(CancellationToken _)
		{
			Console.WriteLine(State);
			return Task.CompletedTask;
		}
	}

	public interface ITestGrain2 : IGrainWithStringKey
	{
		Task Save();
		Task Delete();
	}
}