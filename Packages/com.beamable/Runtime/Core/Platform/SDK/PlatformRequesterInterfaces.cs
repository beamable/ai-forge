using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using System;

namespace Beamable.Api
{
	public interface IPlatformRequester : IRequester
	{
		AccessToken Token { get; set; }
		string TimeOverride { get; set; }
		new string Cid { get; set; }
		new string Pid { get; set; }

		[Obsolete("This field has been removed. Please use the IAuthApi.SetLanguage function instead.")]
		string Language { get; set; }

		IAuthApi AuthService { set; }
		void DeleteToken();
	}



}
