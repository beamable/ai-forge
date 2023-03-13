using Beamable.Api;
using Beamable.Api.Leaderboard;
using Beamable.Common.Api;
using Beamable.Common.Api.Leaderboards;
using Beamable.Serialization.SmallerJSON;
using Beamable.Tests.Runtime;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;

namespace Beamable.Tests.Modules.Leaderboards
{
	public class IncrementTests : BeamableTest
	{
		private const string LEADERBOARD_ID = "leaderboard.test";


		[UnityTest]
		public IEnumerator UpdateIncludesDataInBody_WithoutStats()
		{

			MockRequester.MockRequest<LeaderboardAssignmentInfo>(Method.GET,
				  "/basic/leaderboards/assignment?boardId=leaderboard.test&joinBoard=True")
			   .WithResponse(new LeaderboardAssignmentInfo(LEADERBOARD_ID, MockApi.User.id));

			var req = MockRequester.MockRequest<EmptyResponse>(Method.PUT, $"/object/leaderboards/{LEADERBOARD_ID}/entry")
			   .WithJsonFieldMatch("increment", true)
			   .WithJsonFieldMatch("id", MockApi.User.id)
			   .WithoutJsonField("stats")
			   .WithJsonFieldMatch("score", CheckDouble(10));
			yield return MockApi.LeaderboardService.IncrementScore(LEADERBOARD_ID, 10).ToYielder();
			Assert.IsTrue(req.Called);
		}

		[UnityTest]
		public IEnumerator GiantNumberWorks_WithoutStats()
		{

			MockRequester.MockRequest<LeaderboardAssignmentInfo>(Method.GET,
				  "/basic/leaderboards/assignment?boardId=leaderboard.test&joinBoard=True")
			   .WithResponse(new LeaderboardAssignmentInfo(LEADERBOARD_ID, MockApi.User.id));

			var req = MockRequester.MockRequest<EmptyResponse>(Method.PUT, $"/object/leaderboards/{LEADERBOARD_ID}/entry")
			   .WithJsonFieldMatch("increment", true)
			   .WithJsonFieldMatch("id", MockApi.User.id)
			   .WithoutJsonField("stats")
			   .WithJsonFieldMatch("score", CheckDouble(1e15));
			yield return MockApi.LeaderboardService.IncrementScore(LEADERBOARD_ID, 1e15).ToYielder();
			Assert.IsTrue(req.Called);
		}

		[UnityTest]
		public IEnumerator UpdateIncludesDataInBody_WithStats()
		{

			MockRequester.MockRequest<LeaderboardAssignmentInfo>(Method.GET,
				  "/basic/leaderboards/assignment?boardId=leaderboard.test&joinBoard=True")
			   .WithResponse(new LeaderboardAssignmentInfo(LEADERBOARD_ID, MockApi.User.id));

			var req = MockRequester.MockRequest<EmptyResponse>(Method.PUT, $"/object/leaderboards/{LEADERBOARD_ID}/entry")
			   .WithJsonFieldMatch("increment", true)
			   .WithJsonFieldMatch("id", MockApi.User.id)
			   .WithJsonFieldMatch("stats", (actual) => actual is ArrayDict dict && dict.Count == 0)
			   .WithJsonFieldMatch("score", CheckDouble(10));
			yield return MockApi.LeaderboardService.IncrementScore(LEADERBOARD_ID, 10, new Dictionary<string, object>()).ToYielder();
			Assert.IsTrue(req.Called);
		}

		private Func<object, bool> CheckDouble(double expected)
		{
			return (actual) =>
			{
				var converted = (double)Convert.ChangeType(actual, TypeCode.Double);
				return Math.Abs(converted - expected) < .001;
			};
		}


		protected override void OnSetupBeamable()
		{
			base.OnSetupBeamable();
			MockApi.LeaderboardService =
			   new LeaderboardService(MockPlatform, MockRequester, null, UnityUserDataCache<RankEntry>.CreateInstance);
		}
	}
}
