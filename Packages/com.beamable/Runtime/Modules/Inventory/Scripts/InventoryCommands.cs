using Beamable.Api.Inventory;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Common.Dependencies;
using Beamable.ConsoleCommands;
using Beamable.Content;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Scripting;

namespace Beamable.Inventory
{
	[BeamableConsoleCommandProvider]
	public class InventoryCommands
	{
		private readonly IDependencyProvider _provider;
		private BeamableConsole Console => _provider.GetService<BeamableConsole>();
		private ContentService ContentService => _provider.GetService<ContentService>();
		private InventoryService InventoryService => _provider.GetService<InventoryService>();

		[Preserve]
		public InventoryCommands(IDependencyProvider provider)
		{
			_provider = provider;
		}

		[BeamableConsoleCommand("SET_CURRENCY", "Set currency to a specified value", "SET_CURRENCY <currencyType> <amount>")]
		public string SetCurrency(string[] args)
		{
			if (args.Length < 2)
			{
				return "You need to provide a <currencyType> and an <amount>";
			}

			var currencyType = args[0];
			var amount = args[1];

			Promise<ClientManifest> promise = ContentService.GetManifest("t:currency");
			promise.Then(manifest =>
			{
				List<string> currencies = ParseCurrencies(manifest.entries);

				if (!CheckIfCurrencyExists(currencyType, currencies))
				{
					Console.Log($"Currency with id {currencyType} doesn't exist. {BuildAvailableCurrencies(currencies)}");
					return;
				}

				if (!long.TryParse(amount, out long parsedAmount))
				{
					Console.Log($"Problem with parsing provided amount");
					return;
				}

				if (parsedAmount < 0)
				{
					Console.Log("Provided amount must be a positive value");
					return;
				}

				InventoryService.SetCurrency($"currency.{currencyType}", parsedAmount);
			});

			return String.Empty;
		}

		[BeamableConsoleCommand("ADD_CURRENCY", "Add some amount of specified currency", "ADD_CURRENCY <currencyType> <amount>")]
		public string AddCurrency(string[] args)
		{
			if (args.Length < 2)
			{
				return "You need to provide a <currencyType> and an <amount>";
			}

			string currencyType = args[0];
			string enteredAmount = args[1];

			if (!long.TryParse(enteredAmount, out long parsedAmount))
			{
				string message = "Problem with parsing provided amount";
				Console.Log(message);
				return message;
			}

			Promise<ClientManifest> promise = ContentService.GetManifest("t:currency");
			promise.Then(manifest =>
			{
				List<string> currencies = ParseCurrencies(manifest.entries);

				if (!CheckIfCurrencyExists(currencyType, currencies))
				{
					Console.Log($"Currency with id {currencyType} doesn't exist. {BuildAvailableCurrencies(currencies)}");
					return;
				}

				InventoryService.GetCurrency($"currency.{currencyType}").Then(currentAmount =>
				{
					long totalAmount = currentAmount + parsedAmount;

					if (parsedAmount > 0)
					{
						if (totalAmount < 0)
						{
							totalAmount = long.MaxValue;
						}
					}
					else if (parsedAmount < 0)
					{
						if (totalAmount < 0)
						{
							totalAmount = 0;
						}
					}
					else
					{
						Console.Log("Provided amount must be a positive");
						return;
					}

					InventoryService.SetCurrency($"currency.{currencyType}", totalAmount);
				});
			});

			return String.Empty;
		}

		[BeamableConsoleCommand("GET_CURRENCY", "Get amount of specified currency", "GET_CURRENCY <currencyType>")]
		public string GetCurrency(string[] args)
		{
			if (args.Length < 1)
			{
				return "You need to provide a <currencyType>";
			}

			string currencyType = args[0];

			Promise<ClientManifest> promise = ContentService.GetManifest("t:currency");
			promise.Then(manifest =>
			{
				List<string> currencies = ParseCurrencies(manifest.entries);

				if (!CheckIfCurrencyExists(currencyType, currencies))
				{
					Console.Log($"Currency with id {currencyType} doesn't exist. {BuildAvailableCurrencies(currencies)}");
					return;
				}

				InventoryService.GetCurrency($"currency.{currencyType}").Then(currentAmount =>
				{
					Console.Log($"Amount of currency with id {currencyType} {currentAmount.ToString()}");
				});
			});

			return String.Empty;
		}

		private List<string> ParseCurrencies(List<ClientContentInfo> entries)
		{
			List<string> currencies = new List<string>();

			foreach (ClientContentInfo entry in entries)
			{
				string[] elements = entry.contentId.Split('.');
				currencies.Add(elements[1]);
			}

			return currencies;
		}

		private bool CheckIfCurrencyExists(string currencyType, List<string> currencies)
		{
			return currencies.Contains(currencyType);
		}

		private string BuildAvailableCurrencies(List<string> currencies)
		{
			StringBuilder result = new StringBuilder();
			result.Append("Possible options are: ");

			for (int i = 0; i < currencies.Count; i++)
			{
				result.Append(currencies[i]);
				result.Append(i < currencies.Count - 1 ? ", " : ".");
			}

			return result.ToString();
		}
	}
}
