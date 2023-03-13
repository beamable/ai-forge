using Beamable.Editor.UI.Buss;
using Beamable.UI.Sdf;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static Beamable.Common.Constants.MenuItems.Assets;
using Object = UnityEngine.Object;

namespace Beamable.UI.Buss
{
	[Serializable]
	[CreateAssetMenu(fileName = "BUSSStyleConfig", menuName = "Beamable/BUSS Style",
					 order = Orders.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_2)]
	public class BussStyleSheet : ScriptableObject, ISerializationCallbackReceiver
	{
		public event Action Change;

#pragma warning disable CS0649
		[SerializeField] private List<BussStyleRule> _styles = new List<BussStyleRule>();
		[SerializeField, HideInInspector] private List<Object> _assetReferences = new List<Object>();
		[SerializeField] private bool _isReadOnly;
		[SerializeField] private int _sortingOrder;
#pragma warning restore CS0649

		public List<BussStyleRule> Styles => _styles;
		public bool IsReadOnly => _isReadOnly;
		public int SortingOrder => _sortingOrder;

		public bool IsWritable
		{
			get
			{
#if BEAMABLE_DEVELOPER
				return true;
#else
				return !IsReadOnly;
#endif
			}
		}

		public void TriggerChange()
		{
			if (!IsWritable) return;

			BussConfiguration.UseConfig(conf => conf.UpdateStyleSheet(this));
			Change?.Invoke();
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}

		public void RemoveStyle(BussStyleRule styleRule)
		{
			BeamableUndoUtility.Undo(this, "Remove Style");
			if (_styles.Remove(styleRule))
			{
				TriggerChange();
			}
		}

		public void RemoveStyleProperty(IBussProperty property, BussStyleRule styleRule)
		{
			BussStyleRule bussStyleRule = _styles.Find(style => style == styleRule);
			if (bussStyleRule.RemoveProperty(property))
			{
				TriggerChange();
			}
		}

		public void RemoveAllProperties(BussStyleRule styleRule)
		{
			BeamableUndoUtility.Undo(this, "Clear All");
			styleRule.Properties.Clear();
			TriggerChange();
		}

		public void OnBeforeSerialize()
		{
			PutAssetReferencesInReferenceList();
		}
		private void OnValidate()
		{
			TriggerChange();
		}

		public void OnAfterDeserialize()
		{
			AssignAssetReferencesFromReferenceList();
		}

		private void AssignAssetReferencesFromReferenceList()
		{
			foreach (BussStyleRule style in Styles)
			{
				style.AssignAssetReferencesFromReferenceList(_assetReferences);
			}
		}

		private void PutAssetReferencesInReferenceList()
		{
			_assetReferences.Clear();
			foreach (BussStyleRule style in Styles)
			{
				style.PutAssetReferencesInReferenceList(_assetReferences);
			}
		}

#if BEAMABLE_DEVELOPER
		public void SetReadonly(bool value)
		{
			_isReadOnly = value;
		}

		public void SetSortingOrder(int order)
		{
			_sortingOrder = order;
		}
#endif
	}

	[Serializable]
	public class BussStyleRule : BussStyleDescription, ISerializationCallbackReceiver
	{
		[SerializeField] private string _selector;

		/// <summary>
		/// This property isn't serialized, so it will default to 0 when the object is reloaded from disk.
		/// However, it should be used to force a style rule to the top or bottom of a sorting list.
		/// </summary>
		public int ForcedVisualPriority { get; private set; }
		private static int _nextForcedVisualPriority;

		/// <summary>
		/// Mark the current rule has the most important visual rule in ordering until the next domain reload.
		/// </summary>
		public void SetForcedVisualPriority() => ForcedVisualPriority = ++_nextForcedVisualPriority;

		public BussSelector Selector => BussSelectorParser.Parse(_selector);

		public string SelectorString
		{
			get => _selector;
			set => _selector = value;
		}

		public static BussStyleRule Create(string selector, List<BussPropertyProvider> properties)
		{
			return new BussStyleRule { _selector = selector, _properties = properties };
		}

		public bool RemoveProperty(IBussProperty bussProperty)
		{
			BussPropertyProvider provider = _properties.Find(property => property.GetProperty() == bussProperty);
			return _properties.Remove(provider);
		}

		public void OnBeforeSerialize()
		{
			ForcedVisualPriority = 0;
		}

		public void OnAfterDeserialize()
		{
			ForcedVisualPriority = 0;
		}
	}

	[Serializable]
	public class BussStyleDescription
	{
		// Style card state related data
		[SerializeField] private bool _folded;
		[SerializeField] private bool _showAll;

		[SerializeField] protected List<BussPropertyProvider> _properties = new List<BussPropertyProvider>();
		[SerializeField] protected List<BussPropertyProvider> _cachedProperties = new List<BussPropertyProvider>();
		public List<BussPropertyProvider> Properties => _properties;

		public bool Folded => _folded;
		public bool ShowAll => _showAll;

		public bool HasProperty(string key)
		{
			return _properties.Find(prop => prop.Key == key) != null;
		}

		public bool TryGetCachedProperty(string key, out IBussProperty property)
		{
			BussPropertyProvider provider = _cachedProperties.Find(prop => prop.Key == key);
			property = provider?.GetProperty();
			return property != null;
		}

		public bool CacheProperty(string key, IBussProperty property)
		{
			if (TryGetCachedProperty(key, out _))
			{
				return false;
			}

			BussPropertyProvider provider = BussPropertyProvider.Create(key, property.CopyProperty());
			_cachedProperties.Add(provider);

			CleanupCachedProperties();

			return true;
		}

		public void RemoveCachedProperty(string key)
		{
			var cachedProperty = _cachedProperties.Find(prop => prop.Key == key);
			_cachedProperties.Remove(cachedProperty);

			CleanupCachedProperties();
		}

		private void CleanupCachedProperties()
		{
			var indexesToRemove = new List<int>();

			for (int index = 0; index < _cachedProperties.Count; index++)
			{
				BussPropertyProvider cachedProperty = _cachedProperties[index];
				if (cachedProperty.Key == String.Empty)
				{
					indexesToRemove.Add(index);
				}
			}

			for (int index = _cachedProperties.Count - 1; index >= 0; index--)
			{
				if (indexesToRemove.Contains(index))
				{
					_cachedProperties.RemoveAt(index);
				}
			}
		}

		public void SetFolded(bool value)
		{
			_folded = value;
		}

		public void SetShowAll(bool value)
		{
			_showAll = value;
		}
	}

	[Serializable]
	public class BussPropertyProvider
	{
		[SerializeField, FormerlySerializedAs("key")]
		private string _key;

		[SerializeField, SerializableValueImplements(typeof(IBussProperty)), FormerlySerializedAs("property")]
		private SerializableValueObject _property;

		public string Key => _key;

		public bool IsVariable => BussStyleSheetUtility.IsValidVariableName(Key);
		public bool HasVariableReference => GetProperty() is VariableProperty;

		public BussPropertyValueType ValueType => GetProperty().ValueType;

		public static BussPropertyProvider Create(string key, IBussProperty property, bool forceSerialization = false)
		{
			var propertyProvider = new SerializableValueObject();
			propertyProvider.Set(property);

			if (forceSerialization)
			{
				propertyProvider.ForceSerialization();
			}

			return new BussPropertyProvider { _key = key, _property = propertyProvider };
		}

		public IBussProperty GetProperty()
		{
			return _property.Get<IBussProperty>();
		}

		public void SetProperty(IBussProperty bussProperty)
		{
			_property.Set(bussProperty);
		}

		public bool IsPropertyOfType(Type type)
		{
			return type.IsInstanceOfType(GetProperty());
		}
	}
}
