namespace TestGrains
{
	public interface ITestGrain : IGrainWithStringKey
	{
		Task<string> GetThePhrase();
	}
}