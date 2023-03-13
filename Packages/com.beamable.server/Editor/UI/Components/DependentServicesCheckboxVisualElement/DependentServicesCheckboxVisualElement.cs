using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Model;
using System;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Microservice.UI.Components
{
	public class DependentServicesCheckboxVisualElement : MicroserviceComponent
	{
		public Action<MongoStorageModel, bool> OnServiceRelationChanged;
		public MongoStorageModel MongoStorageModel { get; set; }
		public bool IsServiceRelation
		{
			get => _isServiceRelation;
			private set
			{
				_isServiceRelation = value;
				OnServiceRelationChanged?.Invoke(MongoStorageModel, _isServiceRelation);
			}
		}
		private bool _isServiceRelation;

		private BeamableCheckboxVisualElement _checkbox;

		public DependentServicesCheckboxVisualElement(bool isServiceRelation) : base(nameof(DependentServicesCheckboxVisualElement))
		{
			// Silent set
			_isServiceRelation = isServiceRelation;
		}
		public override void Refresh()
		{
			base.Refresh();
			QueryVisualElements();
			UpdateVisualElements();

		}
		private void QueryVisualElements()
		{
			_checkbox = Root.Q<BeamableCheckboxVisualElement>("checkbox");
		}
		private void UpdateVisualElements()
		{
			_checkbox.Refresh();
			_checkbox.SetWithoutNotify(_isServiceRelation);
			_checkbox.OnValueChanged += state => IsServiceRelation = state;
		}
	}
}
