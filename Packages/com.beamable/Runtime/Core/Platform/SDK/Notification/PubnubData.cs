namespace Beamable.Api.Notification
{
	public class PubNubOp
	{
		public enum PNO
		{
			OpSubscribe,
			OpUnsubscribe,
			OpNone
		};

		public PNO operation;
		public string channel;
		public string presenceChannel;
		public PubnubSubscriptionManager.OnPubNubOperationDelegate onProcessCallback;

		// TODO: Might be worth having some factory methods around create each PubNubOp
		public PubNubOp(PNO operation, string channel,
		   PubnubSubscriptionManager.OnPubNubOperationDelegate onProcessCallback = null) : this(operation, channel, "",
		   onProcessCallback)
		{
		}

		public PubNubOp(PNO operation, string channel, string presenceChannel,
		   PubnubSubscriptionManager.OnPubNubOperationDelegate onProcessCallback = null)
		{
			this.operation = operation;
			this.channel = channel;
			this.presenceChannel = presenceChannel;
			this.onProcessCallback = onProcessCallback;
		}
	}

	public class Channel
	{
		public string channel;

		public Channel(string channel)
		{
			this.channel = channel;
		}
	}

	public class PresenceChannel : Channel
	{
		public PresenceChannel(string channel) : base(channel)
		{
		}
	}
}
