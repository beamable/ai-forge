using Beamable.UI.Buss;
using System;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.UI.Components
{
	public class EnumBussPropertyVisualElement : BussPropertyVisualElement<EnumBussProperty>
	{

		public EnumBussPropertyVisualElement(EnumBussProperty property) : base(property) { }

		private EnumField _field;

		public override void Init()
		{
			base.Init();

			_field = new EnumField();
			AddBussPropertyFieldClass(_field);
			_field.Init(Property.EnumValue);
			Root.Add(_field);

			_field.RegisterValueChangedCallback(OnValueChange);
		}

		private void OnValueChange(ChangeEvent<Enum> changeEvent)
		{
			OnBeforeChange?.Invoke();
			Property.EnumValue = changeEvent.newValue;
			OnValueChanged?.Invoke(Property);
			TriggerStyleSheetChange();
		}

		public override void OnPropertyChangedExternally()
		{
			_field.SetValueWithoutNotify(Property.EnumValue);
		}
	}
}
