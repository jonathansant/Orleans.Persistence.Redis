using Orleans.Providers;
using Orleans.Streams;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Persistence.Redis.E2E
{
	[StorageProvider(ProviderName = "TestingProvider")]
	public class TestGrain : Grain<MockState>, ITestGrain
	{
		private IAsyncStream<string> _stream;

		public Task<MockState> GetTheState()
			=> Task.FromResult(State);

		public async Task SaveMe(MockState mockState)
		{
			State = mockState;
			await WriteStateAsync();
		}

		public Task Deactivate()
		{
			DeactivateOnIdle();
			return Task.CompletedTask;
		}

		public Task DeleteState() 
			=> ClearStateAsync();

		public Task WriteNullToState()
		{
			State = null;
			return WriteStateAsync();
		}

		public override Task OnActivateAsync()
		{
			var provider = GetStreamProvider("TestStream");
			_stream = provider.GetStream<string>(TestBase.StreamGuid, "deactivate-notifications");

			return Task.CompletedTask;
		}

		public override Task OnDeactivateAsync() => _stream.OnNextAsync("done");
	}

	[StorageProvider(ProviderName = "TestingProvider2")]
	public class TestGrain2 : Grain<MockState>, ITestGrain2
	{
		public Task<MockState> GetTheState()
			=> Task.FromResult(State);

		public async Task SaveMe(MockState mockState)
		{
			State = mockState;
			await WriteStateAsync();
		}
	}

	public interface ITestGrain : IGrainWithStringKey
	{
		Task<MockState> GetTheState();
		Task SaveMe(MockState mockState);
		Task Deactivate();
		Task DeleteState();
		Task WriteNullToState();
	}

	public interface ITestGrain2 : IGrainWithStringKey
	{
		Task<MockState> GetTheState();
		Task SaveMe(MockState mockState);
	}

	public class MockState
	{
		private static readonly Random Rand = new Random();

		public int NoHeads { get; set; }
		public string Name { get; set; }

		public static MockState Empty { get; } = new MockState();

		public static MockState Generate()
			=> new MockState
			{
				Name = string.Join(string.Empty, Enumerable.Range(0, 10).Aggregate(string.Empty, (s, i) => s += i)),
				NoHeads = Rand.Next(8)
			};

		public override bool Equals(object obj)
		{
			var comparee = (MockState)obj;
			return comparee.NoHeads == NoHeads
				   && comparee.Name == Name;
		}
	}
}
