using Beamable.UI.Buss;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using Object = UnityEngine.Object;

namespace Beamable.Editor.UI.Components
{
	public class AssetBussPropertyVisualElement : BussPropertyVisualElement<BaseAssetProperty>
	{
		public AssetBussPropertyVisualElement(BaseAssetProperty property) : base(property) { }

		private ObjectField _field;

		public override void Init()
		{
			base.Init();

			_field = new ObjectField();
			AddBussPropertyFieldClass(_field);
			_field.objectType = Property.GetAssetType();
			_field.value = Property.GenericAsset;
			Root.Add(_field);

			_field.RegisterValueChangedCallback(OnValueChange);
		}

		private void OnValueChange(ChangeEvent<Object> evt)
		{
			OnBeforeChange?.Invoke();
			Property.GenericAsset = evt.newValue;
			OnValueChanged?.Invoke(Property);
			TriggerStyleSheetChange();
		}

		public override void OnPropertyChangedExternally()
		{
			_field.SetValueWithoutNotify(Property.GenericAsset);
		}
	}
}
