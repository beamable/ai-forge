using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.UI.Buss
{
	public enum PropertyValueState
	{
		NoResult,
		SingleResult,
		MultipleResults,
		VariableLoopDetected
	}

	public class PropertyReference
	{
		public readonly int HashKey;
		public readonly string Key;
		public readonly BussPropertyProvider PropertyProvider;
		public readonly BussStyleRule StyleRule;
		public readonly BussStyleSheet StyleSheet;

		public PropertyReference() { }

		public PropertyReference(string key,
								 BussStyleSheet styleSheet,
								 BussStyleRule styleRule,
								 BussPropertyProvider propertyProvider)
		{
			HashKey = Animator.StringToHash(key);
			Key = key;
			StyleSheet = styleSheet;
			StyleRule = styleRule;
			PropertyProvider = propertyProvider;
		}

		public SelectorWeight GetWeight()
		{
			return StyleRule == null ? SelectorWeight.Max : StyleRule.Selector.GetWeight();
		}

		public string GetDisplayString()
		{
			return $"{StyleSheet.name} - {StyleRule.SelectorString} -- {this.PropertyProvider.IsVariable}";
		}
	}
}
