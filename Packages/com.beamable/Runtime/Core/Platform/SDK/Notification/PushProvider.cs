namespace Beamable.Api.Notification
{
	public enum PushProvider
	{
		Unknown = 0,
		Apple,
		Google
	}

	public static class PushProviderExtensions
	{
		public static string ToRequestString(this PushProvider self)
		{
			switch (self)
			{
				case PushProvider.Apple:
					return "apple";
				case PushProvider.Google:
					return "google";
				default:
					return "";
			}
		}
	}
}
