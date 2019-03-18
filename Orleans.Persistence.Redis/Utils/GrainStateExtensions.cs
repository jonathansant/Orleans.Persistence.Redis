namespace Orleans.Persistence.Redis.Utils
{
	public static class GrainStateExtensions
	{
		public static IGrainState From(this IGrainState newState, IGrainState other)
		{
			newState.State = other.State;
			newState.ETag = other.ETag;

			return newState;
		}

		public static IGrainState Empty(this IGrainState newState)
		{
			newState.State = null;
			newState.ETag = null;

			return newState;
		}
	}
}
