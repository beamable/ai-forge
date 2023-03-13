using Beamable.AccountManagement;
using Beamable.Common.Api.Auth;
using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Beamable.Tests.Modules.AccountManagement.AccountManagementSignalsTests
{
	public static class TestHelperExtensions
	{
		public static AccountManagementSignals PrepareForTesting(this AccountManagementSignals signaler, GameObject gob, Action<LoadingArg> loadingSetter)
		{
			AccountManagementSignals.SetPending(null, null);
			signaler.Loading = new LoadingEvent();

			signaler.Loading.AddListener(arg => loadingSetter(arg));

			void InitEvent<TSingleArg, TEvent>(string name, out TEvent evt) where TEvent : UnityEvent<TSingleArg>, new()
			{
				evt = new TEvent();
				evt.AddListener(x => Assert.Fail($"unexpected call to event: {name}"));
			}


			InitEvent<User, UserEvent>(nameof(signaler.UserAvailable), out signaler.UserAvailable);
			InitEvent<User, UserEvent>(nameof(signaler.UserAnonymous), out signaler.UserAnonymous);
			InitEvent<User, UserEvent>(nameof(signaler.UserLoggedIn), out signaler.UserLoggedIn);
			InitEvent<User, UserEvent>(nameof(signaler.UserSwitchAvailable), out signaler.UserSwitchAvailable);

			InitEvent<string, EmailEvent>(nameof(signaler.EmailIsAvailable), out signaler.EmailIsAvailable);
			InitEvent<string, EmailEvent>(nameof(signaler.EmailIsInvalid), out signaler.EmailIsInvalid);
			InitEvent<string, EmailEvent>(nameof(signaler.EmailIsRegistered), out signaler.EmailIsRegistered);

			return signaler;
		}
	}
}
