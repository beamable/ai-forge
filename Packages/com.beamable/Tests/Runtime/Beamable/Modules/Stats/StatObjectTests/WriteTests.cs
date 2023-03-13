using Beamable.Api;
using Beamable.Api.Caches;
using Beamable.Api.Stats;
using Beamable.Common.Api;
using Beamable.Common.Api.Stats;
using Beamable.Stats;
using Beamable.Tests.Runtime;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Tests.Modules.Stats.StatObjectTests
{
	public class WriteTests : BeamableTest
	{

		protected override void OnSetupBeamable()
		{
			base.OnSetupBeamable();
			MockApi.StatsService = new StatsService(MockPlatform, MockRequester, null, UnityUserDataCache<Dictionary<string, string>>.CreateInstance, new OfflineCache());
		}

		[Test]
		public void EventFires()
		{
			var so = ScriptableObject.CreateInstance<StatObject>();
			var wasInvoked = false;
			so.StatKey = "statKey";
			so.Access = StatAccess.Private;

			so.OnValueChanged += evt => { wasInvoked = true; };

			MockRequester.MockRequest<EmptyResponse>(Method.POST, null)
			   .WithURIPrefix("/object/stats")
			   .WithBodyMatch<StatUpdates>(_ => true);

			so.Write("a");

			Assert.IsTrue(wasInvoked);
		}
	}
}
