using System;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants;

namespace Beamable.Editor.UI.Components
{
	public class SearchBarVisualElement : BeamableVisualElement
	{
		private TextField _textField;
		private double _lastChangedTime;
		private bool _pendingChange;
		public string Value => _textField.value;
		public event Action<string> OnSearchChanged;

		public new class UxmlFactory : UxmlFactory<SearchBarVisualElement, UxmlTraits> { }

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlStringAttributeDescription customText = new UxmlStringAttributeDescription
			{
				name = "custom-text",
				defaultValue = "nada"
			};

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as SearchBarVisualElement;
			}
		}

		public SearchBarVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(SearchBarVisualElement)}/{nameof(SearchBarVisualElement)}")
		{
			Refresh();
			RegisterCallback<AttachToPanelEvent>(evt =>
			{
				EditorApplication.update += OnEditorUpdate;
			});
		}

		protected override void OnDetach()
		{
			base.OnDetach();
			EditorApplication.update -= OnEditorUpdate;
		}

		public override void Refresh()
		{
			base.Refresh();
			_textField = Root.Q<TextField>();
			_textField.RegisterValueChangedCallback(Textfield_ValueChanged);
		}

		private void OnEditorUpdate()
		{
			if (_pendingChange && EditorApplication.timeSinceStartup > _lastChangedTime + .25f)
			{
				_pendingChange = false;
				OnSearchChanged?.Invoke(_textField.value);
			}
		}

		private void Textfield_ValueChanged(ChangeEvent<string> evt)
		{
			_pendingChange = true;
			_lastChangedTime = EditorApplication.timeSinceStartup;
		}

		public void SetValueWithoutNotify(string value)
		{
			_textField.SetValueWithoutNotify(value);
		}

		public void DoFocus()
		{
#if UNITY_2021_2_OR_NEWER
			EditorApplication.delayCall += _textField.BeamableFocus;
#else
			_textField.BeamableFocus();
#endif
		}
	}
}
