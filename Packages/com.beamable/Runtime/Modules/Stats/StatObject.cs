using Beamable.Common;
using Beamable.Common.Api;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Beamable.Common.Constants.MenuItems.Assets;

namespace Beamable.Stats
{
	public enum StatAccess
	{
		Public, Private
	}

	public static class StatAccessExtensions
	{
		public static string GetString(this StatAccess access)
		{
			switch (access)
			{
				case StatAccess.Private:
					return "private";
				case StatAccess.Public:
					return "public";
				default:
					throw new Exception("unknown stat access");
			}
		}
	}

	[Serializable]
	public class StatObjectChangeEvent
	{
		public StatObject Stat;
		public long UserId;
		public string NewValue;
	}

	[CreateAssetMenu(
		fileName = "Stat",
		menuName = Paths.MENU_ITEM_PATH_ASSETS_BEAMABLE + "/" +
				   "Stat",
		order = Orders.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_3)]
	[Serializable]
	public class StatObject : ScriptableObject
	{
		[Tooltip("The lookup value for a statistic")]
		public string StatKey;

		public StatAccess Access = StatAccess.Public;

		[Tooltip("When a player has no stat, this default will be used instead.")]
		public string DefaultValue;

		public bool ProfanityChecked;

		public event Action<StatObjectChangeEvent> OnValueChanged;

		private List<StatBehaviour> _listeners = new List<StatBehaviour>();

		public Promise<Unit> Write(string value)
		{
			return API.Instance.FlatMap(de =>
			{
				Promise<Unit> profanityPromise;
				if (ProfanityChecked)
				{
					profanityPromise = de.Experimental.ChatService.ProfanityAssert(value)
										 .Map(empty => PromiseBase.Unit);
				}
				else
				{
					profanityPromise = Promise<Unit>.Successful(PromiseBase.Unit);
				}

				return profanityPromise.FlatMap(unit =>
				{
					Promise<EmptyResponse> writeOperation =
						de.StatsService.SetStats(Access.GetString(), new Dictionary<string, string> { { StatKey, value } });

					var changeEvent = new StatObjectChangeEvent { UserId = de.User.id, NewValue = value, Stat = this };

					writeOperation.Then(_ =>
					{
						OnValueChanged?.Invoke(changeEvent);
						_listeners.ForEach(l => l.Refresh());
					});
					return writeOperation.Map(_ => PromiseBase.Unit);
				});
			});
		}

		public void Attach(StatBehaviour behaviour)
		{
			if (!_listeners.Contains(behaviour))
			{
				_listeners.Add(behaviour);
			}
		}

		public void Detach(StatBehaviour behaviour)
		{
			_listeners.Remove(behaviour);
		}
	}
}
