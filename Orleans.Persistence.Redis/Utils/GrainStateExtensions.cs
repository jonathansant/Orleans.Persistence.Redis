namespace Orleans.Persistence.Redis.Utils
{
	public static class GrainStateExtensions
	{
		public static IGrainState<T> From<T>(this IGrainState<T> newState, IGrainState<T> other)
		{
			newState.State = other.State;
			newState.ETag = other.ETag;

			return newState;
		}

		public static IGrainState<T> Empty<T>(this IGrainState<T> newState)
		{
			newState.State = default;
			newState.ETag = null;

			return newState;
		}
	}
}
