using Beamable.Editor.UI.Buss;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beamable.UI.Buss
{
	[ExecuteAlways, DisallowMultipleComponent]
	public class BussElement : MonoBehaviour, ISerializationCallbackReceiver
	{
		public event Action Change;
		public event Action StyleRecalculated;

#pragma warning disable CS0649
		[SerializeField, BussId] private string _id;
		[SerializeField, BussClass] private List<string> _classes = new List<string>();
		[SerializeField] private BussStyleDescription _inlineStyle;
		[SerializeField] private BussStyleSheet _styleSheet;
		[SerializeField, HideInInspector] private List<Object> _assetReferences = new List<Object>();
		[SerializeField, HideInInspector] private BussElement _parent;
		[SerializeField, HideInInspector] private List<BussElement> _children = new List<BussElement>();
#pragma warning restore CS0649

		private readonly List<string> _pseudoClasses = new List<string>();
		public List<BussStyleSheet> AllStyleSheets { get; } = new List<BussStyleSheet>();
		public BussStyle Style { get; } = new BussStyle();

		private PropertySourceTracker _sources;
		public PropertySourceTracker Sources => _sources ?? (_sources = new PropertySourceTracker(this));

		public string Id
		{
			get => _id;
			set
			{
				_id = value;
				OnStyleChanged();
			}
		}

		public IEnumerable<string> Classes => _classes;
		public BussStyleDescription InlineStyle => _inlineStyle;

		public virtual string TypeName => "BussElement";

		public BussStyleSheet StyleSheet
		{
			get => _styleSheet;
			set
			{
				_styleSheet = value;
				OnStyleChanged();
			}
		}

		public BussElement Parent => _parent;

		private IReadOnlyList<BussElement> _childrenReadOnly;

		public IReadOnlyList<BussElement> Children
		{
			get
			{
				if (_childrenReadOnly == null)
				{
					if (_children == null)
					{
						_children = new List<BussElement>();
					}

					_childrenReadOnly = _children.AsReadOnly();
				}

				return _childrenReadOnly;
			}
		}

		public virtual void ApplyStyle() { }

		#region Unity Callbacks

		private void OnEnable()
		{
			CheckParent();
			CheckRelationToSiblings();
			OnStyleChanged();
		}

		private void Reset()
		{
			CheckParent();
			OnStyleChanged();
		}

		private void OnValidate()
		{
			if (!gameObject || !gameObject.scene.IsValid())
			{
				return; // OnValidate runs on prefabs, which we absolutely don't want to.
			}

			CheckParent();
			OnStyleChanged();
		}

		private void OnTransformParentChanged()
		{
			CheckParent();
			CheckRelationToSiblings();
			OnStyleChanged();
		}

		private void OnDisable()
		{
			if (Parent != null)
			{
				Parent._children.Remove(this);
			}
			else
			{
				BussConfiguration.UseConfig(c => c.UnregisterObserver(this));
			}
		}

		#endregion

		#region Changing Classes

		public void UpdateClasses(IEnumerable<string> newClasses)
		{
			_classes.Clear();
			_classes = new List<string>(newClasses);
			RecalculateStyle();
		}

		public void AddClass(string className)
		{
			if (!_classes.Contains(className))
			{
				_classes.Add(className);
				RecalculateStyle();
			}
		}

		public void RemoveClass(string className)
		{
			if (_classes.Remove(className))
			{
				RecalculateStyle();
			}
		}

		public void SetClass(string className, bool isEnabled)
		{
			if (isEnabled)
			{
				AddClass(className);
			}
			else
			{
				RemoveClass(className);
			}
		}

		// TODO: Disabled with BEAM-3130 due to incomplete implementation
		// public void SetPseudoClass(string className, bool isEnabled)
		// {
		// 	var changed = false;
		// 	if (isEnabled)
		// 	{
		// 		if (!_pseudoClasses.Contains(className))
		// 		{
		// 			_pseudoClasses.Add(className);
		// 			changed = true;
		// 		}
		// 	}
		// 	else
		// 	{
		// 		changed = _pseudoClasses.Remove(className);
		// 	}
		//
		// 	if (changed)
		// 	{
		// 		// TODO: Disabled with BEAM-3130 due to incomplete implementation
		// 		//Style.SetStyleAnimatedListener(ApplyStyle);
		// 		//Style.SetPseudoStyle(className, isEnabled);
		// 	}
		// }

		#endregion

		public void Reenable()
		{
			enabled = false;
			enabled = true;
		}

		/// <summary>
		/// Used when the parent or the style sheet is changed.
		/// Recalculates the list of style sheets that are used for this BussElement and then recalculates BussStyle.
		/// </summary>
		public void OnStyleChanged()
		{
			RecalculateStyleSheets();
			RecalculateStyle();
		}

		private void RecalculateStyleSheets()
		{
			var hash = GetStyleSheetHash();
			AllStyleSheets.Clear();
			AddParentStyleSheets(this);
			if (hash != GetStyleSheetHash())
			{
				Change?.Invoke();
			}
		}

		private int GetStyleSheetHash()
		{
			unchecked
			{
				var hash = 0;
				foreach (BussStyleSheet sheet in AllStyleSheets)
				{
					hash = (hash << 3) + sheet.GetHashCode();
				}

				return hash;
			}
		}

		private void AddParentStyleSheets(BussElement element)
		{
			if (element.Parent != null)
			{
				AddParentStyleSheets(element.Parent);
			}

			if (element.StyleSheet != null)
			{
				AllStyleSheets.Remove(element.StyleSheet);
				AllStyleSheets.Add(element.StyleSheet);
			}
		}

		/// <summary>
		/// Recalculates style for this and children BussElements.
		/// </summary>
		public void RecalculateStyle()
		{
			BussConfiguration.UseConfig(c => c.RecalculateStyle(this));
			ApplyStyle();

			StyleRecalculated?.Invoke();

			foreach (BussElement child in Children)
			{
				if (child != null)
				{
					child.OnStyleChanged();
				}
			}
		}

		public void CheckParent()
		{
			var mysTransform = transform;
			var foundParent = (mysTransform == null || mysTransform.parent == null)
				? null
				: transform.parent.GetComponentInParent<BussElement>();

			if (Parent != null)
			{
				Parent._children.Remove(this);
			}
			else
			{
				BussConfiguration.UseConfig(c => c.UnregisterObserver(this));
			}

			_parent = foundParent;
			if (Parent == null)
			{
				BussConfiguration.UseConfig(c => c.RegisterObserver(this));
			}
			else
			{
				if (!Parent._children.Contains(this))
				{
					Parent._children.Add(this);
				}
			}
		}

		private void CheckRelationToSiblings()
		{
			if (Parent == null)
			{
				BussConfiguration.UseConfig(c => CheckRelations(c.RootBussElements));
			}
			else
			{
				CheckRelations(Parent.Children);
			}
		}

		private void CheckRelations(IEnumerable<BussElement> elements)
		{
			foreach (BussElement element in elements.ToArray())
			{
				if (element != this && element != null)
				{
					element.CheckParent();
				}
			}
		}

		public void OnBeforeSerialize()
		{
			_assetReferences.Clear();
			_inlineStyle.PutAssetReferencesInReferenceList(_assetReferences);
		}

		public void OnAfterDeserialize()
		{
			_inlineStyle.AssignAssetReferencesFromReferenceList(_assetReferences);
		}
	}
}
