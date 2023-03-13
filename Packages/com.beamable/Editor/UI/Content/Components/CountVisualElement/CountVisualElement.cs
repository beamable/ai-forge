using System.Collections.Generic;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
namespace Beamable.Editor.Content.Components
{
	public class CountVisualElement : ContentManagerComponent
	{
		private Label _countLabel;
		private int _value;

		public int Value
		{
			get => _value;
			set
			{
				SetValue(value);
			}
		}

		private bool _isDangerous;

		public bool IsDangerous
		{
			get => _isDangerous;
			set
			{
				_isDangerous = value;
				SetValue(Value);
			}
		}

		public new class UxmlFactory : UxmlFactory<CountVisualElement, UxmlTraits>
		{
		}
		public CountVisualElement() : base(nameof(CountVisualElement))
		{
		}

		public override void Refresh()
		{
			base.Refresh();
			_countLabel = Root.Q<Label>();

			SetValue(_value);
		}

		public void SetValue(int count)
		{
			_value = count;
			_countLabel.text = count.ToString();

			if (_isDangerous && count > 0)
			{
				AddToClassList("danger");
			}
			else
			{
				RemoveFromClassList("danger");
			}
		}

		public new class UxmlTraits : VisualElement.UxmlTraits
		{

			private UxmlBoolAttributeDescription isDangerous = new UxmlBoolAttributeDescription
			{
				name = "is-dangerous",
				defaultValue = false
			};

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as CountVisualElement;
				self._isDangerous = isDangerous.GetValueFromBag(bag, cc);
				self.Refresh();
			}
		}
	}
}
