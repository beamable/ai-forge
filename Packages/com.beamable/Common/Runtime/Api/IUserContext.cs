namespace Beamable.Common.Api
{
	public interface IUserContext
	{
		/// <summary>
		/// The current gamertag of this context
		/// </summary>
		long UserId { get; }
	}

	public class SimpleUserContext : IUserContext
	{
		public long UserId { get; }

		public SimpleUserContext(long userId)
		{
			UserId = userId;
		}
	}
}
