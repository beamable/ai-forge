using Beamable.Api.Payments;
using Beamable.Common;
using Beamable.Common.Api.Content;
using Beamable.Common.Dependencies;
using Beamable.Platform.Tests;
using Beamable.Platform.Tests.Content;
using NUnit.Framework;
using System;

namespace Beamable.Tests.Runtime
{
	public class BeamContextTest
	{
		protected MockBeamContext Context;
		protected MockContentService MockContent;
		protected MockPlatformAPI Requester;

		[SetUp]
		public void Setup()
		{
			MockContent = new MockContentService();
			ContentApi.Instance = Promise<IContentApi>.Successful(MockContent);

		}

		protected void TriggerContextInit(Action<IDependencyBuilder> buildDelegate = null, Action<MockBeamContext> initDelegate = null)
		{
			Context = MockBeamContext.Create(
				mutateDependencies: buildDelegate ?? OnRegister,
				onInit: initDelegate ?? OnInit
			);

			Requester = Context.Requester;
		}


		[TearDown]
		public void Cleanup()
		{
			Context.ClearPlayerAndStop();
		}

		protected virtual void OnInit(MockBeamContext ctx)
		{
			ctx.AddStandardGuestLoginRequests()
			   .AddPubnubRequests()
			   .AddSessionRequests();
		}

		protected virtual void OnRegister(IDependencyBuilder builder)
		{
			builder.RemoveIfExists<IBeamablePurchaser>();
			builder.RemoveIfExists<IContentApi>();
			builder.AddSingleton<IContentApi>(MockContent);
		}
	}
}
