using Beamable.UI.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.UI.Buss
{
	public partial class BussStyle
	{
		private Action _styleAnimatedAction;
		private Dictionary<string, BussPseudoStyle> PseudoStyles { get; set; }

		protected BussStyle _inheritedFromStyle;

		/// <summary>
		/// Set all the styles in the current instance to the inheritable values from the given style.
		/// If the current style already has assigned properties that don't exist in the next given style,
		/// then the those properties will be removed.
		///
		/// https://www.w3.org/TR/CSS21/propidx.html
		/// </summary>
		/// <param name="other">Some other <see cref="BussStyle"/> element. </param>
		public void Inherit(BussStyle other)
		{
			Clear(); // always clear out the style so that we start clean.
			if (other == null) return;
			_inheritedFromStyle = other;


			foreach (var kvp in other._properties)
			{
				if (!BussStyle.TryGetBinding(kvp.Key, out var binding)) continue; // invalid property;
				if (binding.Inheritable)
				{
					this[kvp.Key] = kvp.Value;
				}
			}
			// TODO: how to clone pseudo styles?
		}

		public IBussProperty this[string key]
		{
			get
			{
				if (_properties.TryGetValue(key, out var property))
				{
					return property;
				}

				return null;
			}

			set
			{
				_properties[key] = value;
			}
		}

		public IBussProperty this[string pseudoClass, string key]
		{
			get
			{
				if (PseudoStyles != null && PseudoStyles.TryGetValue(pseudoClass, out var pseudoStyle))
				{
					return pseudoStyle[key];
				}

				return null;
			}

			set
			{
				if (PseudoStyles == null)
				{
					PseudoStyles = new Dictionary<string, BussPseudoStyle>();
				}

				if (!PseudoStyles.TryGetValue(pseudoClass, out var pseudoStyle))
				{
					pseudoStyle = PseudoStyles[pseudoClass] = new BussPseudoStyle();
				}

				pseudoStyle[key] = value;
			}
		}

		public static IEnumerable<string> Keys => _bindings.Keys;

		public static bool IsKeyValid(string key) => _bindings.ContainsKey(key);

		public static Type GetBaseType(string key)
		{
			if (_bindings.TryGetValue(key, out var binding))
			{
				return binding.PropertyType;
			}

			return typeof(IBussProperty);
		}

		public static IBussProperty GetDefaultValue(string key)
		{
			if (_bindings.TryGetValue(key, out var binding))
			{
				return binding.GetDefaultValue();
			}

			return null;
		}

		public void Clear()
		{
			_inheritedFromStyle = null;
			_properties.Clear();
			PseudoStyles?.Clear();
		}

		public BussStyle GetCombinedStyle()
		{
			var style = this;
			if (PseudoStyles != null)
			{
				foreach (BussPseudoStyle pseudoStyle in PseudoStyles.Values)
				{
					style = pseudoStyle.MergeWithBaseStyle(style);
				}
			}

			return style;
		}

		// TODO: Disabled with BEAM-3130 due to incomplete implementation
		// public void SetPseudoStyle(string pseudoClass, bool enabled, bool withTransition = true)
		// {
		// 	if (PseudoStyles != null && PseudoStyles.TryGetValue(pseudoClass, out var style))
		// 	{
		// 		var transitionDuration = TransitionDuration.Get(this).FloatValue;
		// 		if (withTransition && transitionDuration > 0f)
		// 		{
		// 			style.Enabled = true;
		// 			var easing = TransitionEasing.Get(this).Enum;
		// 			if (style.Tween == null)
		// 			{
		// 				style.Tween = new FloatTween(t =>
		// 				{
		// 					style.BlendValue = t;
		// 					OnStyleAnimated();
		// 				});
		// 				style.Tween.CompleteEvent += () =>
		// 				{
		// 					style.Enabled = style.BlendValue > .5f;
		// 					OnStyleAnimated();
		// 				};
		// 			}
		// 			else
		// 			{
		// 				style.Tween.Stop();
		// 			}
		//
		// 			var tween = style.Tween;
		// 			tween.SetDuration(transitionDuration);
		// 			tween.StartValue = style.BlendValue;
		// 			tween.EndValue = enabled ? 1f : 0f;
		// 			tween.SetEasing(easing);
		// 			tween.Run();
		// 		}
		// 		else
		// 		{
		// 			style.Enabled = enabled;
		// 			style.BlendValue = enabled ? 1f : 0f;
		// 			OnStyleAnimated();
		// 		}
		// 	}
		// }

		public void SetStyleAnimatedListener(Action listener)
		{
			_styleAnimatedAction = listener;
		}

		// TODO: Disabled with BEAM-3130 due to incomplete implementation
		// private void OnStyleAnimated()
		// {
		// 	try
		// 	{
		// 		_styleAnimatedAction?.Invoke();
		// 	}
		// 	catch (Exception e)
		// 	{
		// 		Debug.LogException(e);
		// 		_styleAnimatedAction = null;
		// 	}
		// }
	}
}
