using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Mail;
using Beamable.Common.Dependencies;

namespace Beamable.Api.Mail
{

	public class MailSubscription : PlatformSubscribable<MailQueryResponse, MailQueryResponse>
	{
		public MailSubscription(IDependencyProvider provider) : base(provider, AbsMailApi.SERVICE_NAME)
		{
		}

		protected override void OnRefresh(MailQueryResponse data)
		{
			Notify(data);
		}
	}

	/// <summary>
	/// This type defines the %Client main entry point for the %Mail feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/mail-feature">Mail</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class MailService : AbsMailApi, IHasPlatformSubscriber<MailSubscription, MailQueryResponse, MailQueryResponse>
	{
		public MailSubscription Subscribable { get; }

		public MailService(IPlatformService platform, IBeamableRequester requester, IDependencyProvider provider) : base(requester, platform)
		{
			Subscribable = new MailSubscription(provider);
		}

		public override Promise<MailQueryResponse> GetCurrent(string scope = "") => Subscribable.GetCurrent(scope);
	}
}
