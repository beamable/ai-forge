using Beamable.Api.Payments;
using Beamable.Common.Dependencies;
using Beamable.ConsoleCommands;
using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Beamable.Purchasing
{

	[BeamableConsoleCommandProvider]
	public class PaymentCommands
	{
		private readonly IDependencyProvider _provider;
		private BeamableConsole Console => _provider.GetService<BeamableConsole>();
		private PaymentService PaymentService => _provider.GetService<PaymentService>();

		[Preserve]
		public PaymentCommands(IDependencyProvider provider)
		{
			_provider = provider;
		}

		[BeamableConsoleCommand("TRACK_PAYMENT", "Track a test payment audit", "TRACK_PAYMENT")]
		private string TrackPurchase(string[] args)
		{
			var obtainCurrency = new List<ObtainCurrency>();
			var obtainItems = new List<ObtainItem>();

			var currency = new ObtainCurrency();
			currency.symbol = "coins";
			currency.amount = 100;

			obtainCurrency.Add(currency);

			var request = new TrackPurchaseRequest(
			  "bundle_of_coins",
			  "offer_t10",
			  "com.beamable.test.offer_t10",
			  "main",
			  9.99,
			  "USD",
			  obtainCurrency,
			  obtainItems
			);

			PaymentService.Track(request).Then(_ =>
			{
				Console.Log("Purchase Tracked");
			});

			return String.Empty;
		}
	}
}
