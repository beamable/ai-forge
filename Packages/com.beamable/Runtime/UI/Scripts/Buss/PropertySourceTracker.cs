using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using PropertyReference = Beamable.UI.Buss.PropertyReference;

namespace Beamable.UI.Buss
{
	public class PropertySourceTracker
	{
		private readonly Dictionary<string, SourceData> _sources = new Dictionary<string, SourceData>();
		public BussElement Element { get; }

		public PropertySourceTracker(BussElement element)
		{
			Element = element;
			Recalculate();
		}

		public void Recalculate()
		{
			_sources.Clear();

			if (BussConfiguration.OptionalInstance.HasValue)
			{
				var config = BussConfiguration.OptionalInstance.Value;
				if (config != null)
				{
					foreach (BussStyleSheet styleSheet in config.FactoryStyleSheets)
					{
						AddStyleSheet(styleSheet);
					}

					foreach (BussStyleSheet styleSheet in config.DeveloperStyleSheets)
					{
						AddStyleSheet(styleSheet);
					}
				}
			}

			foreach (BussStyleSheet bussStyleSheet in Element.AllStyleSheets)
			{
				if (bussStyleSheet != null)
				{
					AddStyleSheet(bussStyleSheet);
				}
			}

			AddStyleDescription(null, Element.InlineStyle);
		}

		public bool IsUsed(string key, BussStyleRule styleRule)
		{
			var data = _sources[key];
			return data.Properties.First().StyleRule == styleRule;
		}

		public IEnumerable<string> GetKeys() => _sources.Keys;

		/// <summary>
		/// Enumerate all <see cref="PropertyReference"/> objects of the given key
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public IEnumerable<PropertyReference> GetAllSources(string key)
		{
			if (!_sources.TryGetValue(key, out var sourceData))
			{
				yield break;
			}

			foreach (var reference in sourceData.Properties)
			{
				yield return reference;
			}
		}

		public IEnumerable<string> GetAllVariableNames()
		{
			return _sources.Keys.Where(BussStyleSheetUtility.IsValidVariableName);
		}

		public IEnumerable<string> GetAllVariableNames(Type baseType)
		{
			foreach (var kvp in _sources)
			{
				if (!BussStyleSheetUtility.IsValidVariableName(kvp.Key)) continue;

				var firstProperty = kvp.Value.Properties.FirstOrDefault();
				if (firstProperty == null) continue;

				if (firstProperty.PropertyProvider.IsPropertyOfType(baseType))
				{
					yield return kvp.Key;
				}
			}
		}

		/// <summary>
		/// This method should accept a <see cref="BussPropertyProvider"/> that has an inherited value type.
		/// Given an inherited property provider, this will find the effective provider beyond the given property.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public BussPropertyProvider GetNextInheritedProperty(BussPropertyProvider property)
		{
			if (property.ValueType != BussPropertyValueType.Inherited) return property;

			var found = false;
			foreach (var reference in GetAllSources(property.Key))
			{
				if (found)
				{
					if (reference.PropertyProvider.ValueType == BussPropertyValueType.Inherited) continue;
					var inheritedReference = reference.PropertyProvider;

					if (inheritedReference.HasVariableReference)
					{
						var variableProperty = inheritedReference.GetProperty() as VariableProperty;
						var variableName = variableProperty.VariableName;

						var referenceValue = GetUsedPropertyProvider(variableName, out _);
						return referenceValue;
					}
					else
					{
						return reference.PropertyProvider;
					}

				}
				if (reference.PropertyProvider == property)
				{
					found = true;
				}
			}

			return null;
		}

		public BussPropertyProvider GetUsedPropertyProvider(string key, out int rank)
		{
			return GetUsedPropertyProvider(key, BussStyle.GetBaseType(key), false, out rank);
		}

		public BussPropertyProvider ResolveVariableProperty(string key)
		{
			return GetUsedPropertyProvider(key, BussStyle.GetBaseType(key), true, out _);
		}

		public BussPropertyProvider GetUsedPropertyProvider(string key, Type baseType, bool resolveVariables, out int rank)
		{
			rank = 0;
			if (_sources.ContainsKey(key))
			{
				foreach (var reference in _sources[key].Properties)
				{
					rank++;
					// if the reference says, "inherit", then the used property should continue up the reference sequence
					if (reference.PropertyProvider.ValueType == BussPropertyValueType.Inherited) continue;

					// if the reference is a variable, redirect!
					if (resolveVariables && reference.PropertyProvider.HasVariableReference)
					{
						var variableProperty = reference.PropertyProvider.GetProperty() as VariableProperty;
						var variableName = variableProperty.VariableName;

						var referenceValue = GetUsedPropertyProvider(variableName, out var nestedRank);
						return referenceValue;
					}

					if (reference.PropertyProvider.IsPropertyOfType(baseType) ||
						reference.PropertyProvider.IsPropertyOfType(typeof(VariableProperty)))
					{
						return reference.PropertyProvider;
					}
				}
			}

			return null;
		}

		public PropertyReference GetUsedPropertyReference(string key)
		{
			return GetUsedPropertyReference(key, BussStyle.GetBaseType(key));
		}

		private PropertyReference GetUsedPropertyReference(string key, Type baseType)
		{
			if (_sources.ContainsKey(key))
			{
				foreach (var reference in _sources[key].Properties)
				{
					if (reference.PropertyProvider.IsPropertyOfType(baseType) ||
						reference.PropertyProvider.IsPropertyOfType(typeof(VariableProperty)))
					{
						return reference;
					}
				}
			}

			return new PropertyReference();
		}

		private void AddStyleSheet(BussStyleSheet styleSheet)
		{
			if (styleSheet == null) return;
			foreach (BussStyleRule styleRule in styleSheet.Styles)
			{
				if (styleRule.Selector?.IsElementIncludedInSelector(Element) ?? false)
				{
					AddStyleDescription(styleSheet, styleRule);
				}
			}
		}

		private void AddStyleDescription(BussStyleSheet styleSheet, BussStyleDescription styleDescription)
		{
			if (styleDescription == null || styleDescription.Properties == null) return;
			foreach (BussPropertyProvider property in styleDescription.Properties)
			{
				AddPropertySource(styleSheet, styleDescription as BussStyleRule, property);
			}
		}

		private void AddPropertySource(BussStyleSheet styleSheet,
									   BussStyleRule styleRule,
									   BussPropertyProvider propertyProvider)
		{
			var key = propertyProvider.Key;

			var exactMatch = true;
			if (styleRule != null)
			{
				if (!styleRule.Selector.IsElementIncludedInSelector(Element, out exactMatch))
				{
					// this is an inherited property, but maybe the property isn't inheritable?
					if (BussStyle.TryGetBinding(key, out var binding) && !binding.Inheritable) return;
				}
			}


			var propertyReference = new PropertyReference(key, styleSheet, styleRule, propertyProvider);
			if (!_sources.TryGetValue(key, out SourceData sourceData))
			{
				_sources[key] = sourceData = new SourceData(key);
			}

			sourceData.AddSource(propertyReference, exactMatch);
		}

		public class SourceData
		{
			public readonly string key;
			public readonly List<PropertyReference> Properties = new List<PropertyReference>();


			private readonly List<PropertyReference> InheritedProperties = new List<PropertyReference>();
			private readonly List<PropertyReference> MatchedProperties = new List<PropertyReference>();

			public SourceData(string key)
			{
				this.key = key;
			}

			public void AddSource(PropertyReference propertyReference, bool exactMatch)
			{
				var propList = MatchedProperties;
				if (!exactMatch)
				{
					propList = InheritedProperties;
				}

				var weight = propertyReference.GetWeight();
				var index = propList.FindIndex(r => weight.CompareTo(r.GetWeight()) >= 0);
				if (index < 0)
				{
					propList.Add(propertyReference);
				}
				else
				{
					propList.Insert(index, propertyReference);
				}

				Properties.Clear();
				Properties.AddRange(MatchedProperties);
				Properties.AddRange(InheritedProperties);
			}
		}

	}

	public class PropertySourceDatabase
	{
		private readonly Dictionary<BussElement, PropertySourceTracker> _trackers =
			new Dictionary<BussElement, PropertySourceTracker>();

		public PropertySourceTracker GetTracker(BussElement bussElement)
		{
			if (bussElement == null) return null;
			var tracker = bussElement.Sources;
			return tracker;
		}

		public void Discard()
		{
			foreach (KeyValuePair<BussElement, PropertySourceTracker> pair in _trackers)
			{
				if (pair.Key != null)
				{
					pair.Key.StyleRecalculated -= pair.Value.Recalculate;
				}
			}

			_trackers.Clear();
		}
	}
}
