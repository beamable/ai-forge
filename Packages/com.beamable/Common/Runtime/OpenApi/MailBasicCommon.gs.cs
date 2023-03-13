
namespace Beamable.Api.Autogenerated.Mail
{
	using Beamable.Api.Autogenerated.Models;
	using Beamable.Common;
	using Beamable.Common.Content;
	using IBeamableRequester = Beamable.Common.Api.IBeamableRequester;
	using Method = Beamable.Common.Api.Method;

	public partial interface IMailApi
	{
	}
	public partial class MailApi : IMailApi
	{
		private IBeamableRequester _requester;
		public MailApi(IBeamableRequester requester)
		{
			this._requester = requester;
		}
	}
}
