using Beamable.UI.Buss;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.UI.Components
{
	public class Vector2BussPropertyVisualElement : BussPropertyVisualElement<Vector2BussProperty>
	{
		private FloatField _fieldX, _fieldY;

		public Vector2BussPropertyVisualElement(Vector2BussProperty property) : base(property) { }

		public override void Init()
		{
			base.Init();

			_fieldX = new FloatField();
			AddBussPropertyFieldClass(_fieldX);
			_fieldX.value = Property.Vector2Value.x;
			Root.Add(_fieldX);

			_fieldY = new FloatField();
			AddBussPropertyFieldClass(_fieldY);
			_fieldY.value = Property.Vector2Value.y;
			Root.Add(_fieldY);

			_fieldX.RegisterValueChangedCallback(OnValueChange);
			_fieldY.RegisterValueChangedCallback(OnValueChange);
		}

		private void OnValueChange(ChangeEvent<float> evt)
		{
			OnBeforeChange?.Invoke();
			Property.Vector2Value = new Vector2(
				_fieldX.value,
				_fieldY.value);

			OnValueChanged?.Invoke(Property);

			TriggerStyleSheetChange();
		}

		public override void OnPropertyChangedExternally()
		{
			var value = Property.Vector2Value;
			_fieldX.SetValueWithoutNotify(value.x);
			_fieldY.SetValueWithoutNotify(value.y);
		}
	}
}
