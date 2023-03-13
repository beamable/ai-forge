using Beamable.Common.Dependencies;
using Beamable.ConsoleCommands;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Beamable.Api.Inventory
{
	/// <summary>
	/// This type defines the %Inventory feature's console commands.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[BeamableConsoleCommandProvider]
	public class InventoryConsoleCommands
	{
		private readonly IDependencyProvider _provider;
		private BeamableConsole Console => _provider.GetService<BeamableConsole>();
		private InventoryService Inventory => _provider.GetService<InventoryService>();

		[Preserve]
		public InventoryConsoleCommands(IDependencyProvider provider)
		{
			_provider = provider;
		}

		[BeamableConsoleCommand("CURRENCY-PREVIEW", "Preview currency gain and any bonuses that might result from VIP", "CURRENCY-PREVIEW")]
		protected string PreviewCurrency(params string[] args)
		{
			var currencies = new Dictionary<string, long>()
			{
				{"currency.gems", 100 }
			};

			Inventory.PreviewCurrencyGain(currencies).Then(response =>
			{
				string json = JsonUtility.ToJson(response);
				Debug.Log($"Preview Currency Gain: {json}");
			});

			return "Previewing currency gain...";
		}

		[BeamableConsoleCommand("CURRENCY-MULTIPLIERS", "Get the currency multipliers for this player", "CURRENCY-MULTIPLIERS")]
		protected string GetMultipliers(params string[] args)
		{
			Inventory.GetMultipliers().Then(response =>
			{
				string json = JsonUtility.ToJson(response);
				Debug.Log($"Currency Multipliers: {json}");
			});

			return "Fetching currency multipliers...";
		}

	}

}
