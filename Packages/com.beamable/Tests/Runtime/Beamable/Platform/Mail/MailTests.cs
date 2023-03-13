using Beamable.Common.Api;
using Beamable.Common.Api.Mail;
using Beamable.Content.Utility;
using NUnit.Framework;
using System;

namespace Beamable.Platform.Tests.Mail
{
	public class MailTests
	{
		[Test]
		public void ValidSendMailExpiration()
		{
			string isoDateString = "2022-09-14T20:00:00Z";
			var expiresOffset = DateTimeOffset.Parse(isoDateString);
			var expiresMillis = expiresOffset.ToUnixTimeMilliseconds();

			var mailSendEntry = new MailSendEntry();
			Assert.AreEqual(null, mailSendEntry.expires);

			mailSendEntry.SetExpiration(isoDateString);
			Assert.AreEqual(isoDateString, mailSendEntry.expires);

			mailSendEntry.expires = null;
			mailSendEntry.SetExpiration(expiresMillis);
			Assert.AreEqual(isoDateString, mailSendEntry.expires);

			mailSendEntry.expires = null;
			mailSendEntry.SetExpiration(expiresOffset);
			Assert.AreEqual(isoDateString, mailSendEntry.expires);

			mailSendEntry.expires = null;
			var now = DateTimeOffset.UtcNow;
			mailSendEntry.SetExpiresIn(TimeSpan.FromHours(1.0));
			var inAnHour = now.AddHours(1.0);
			Assert.AreEqual(inAnHour.ToString(DateUtility.ISO_FORMAT), mailSendEntry.expires);
		}

		[Test]
		public void ValidUpdateMailExpiration()
		{
			string isoDateString = "2022-09-14T20:00:00Z";
			var expiresOffset = DateTimeOffset.Parse(isoDateString);
			var expiresMillis = expiresOffset.ToUnixTimeMilliseconds();

			var mailUpdate = new MailUpdate(1L, MailState.Read, true);
			Assert.AreEqual(null, mailUpdate.expires);

			mailUpdate.SetExpiration(isoDateString);
			Assert.AreEqual(isoDateString, mailUpdate.expires);

			mailUpdate.expires = null;
			mailUpdate.SetExpiration(expiresMillis);
			Assert.AreEqual(isoDateString, mailUpdate.expires);

			mailUpdate.expires = null;
			mailUpdate.SetExpiration(expiresOffset);
			Assert.AreEqual(isoDateString, mailUpdate.expires);

			mailUpdate.expires = null;
			var now = DateTimeOffset.UtcNow;
			mailUpdate.SetExpiresIn(TimeSpan.FromHours(1.0));
			var inAnHour = now.AddHours(1.0);
			Assert.AreEqual(inAnHour.ToString(DateUtility.ISO_FORMAT), mailUpdate.expires);
		}
	}
}
