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
	public class TagVisualElement : ContentManagerComponent
	{

		public new class UxmlFactory : UxmlFactory<TagVisualElement, UxmlTraits> { }
		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlStringAttributeDescription customText = new UxmlStringAttributeDescription { name = "custom-text", defaultValue = "nada" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}
			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as TagVisualElement;
			}
		}

		private VisualElement _backGroundElement;
		private Label _label;

		public string Text { get; set; }
		public bool IsLocalOnly { get; set; }
		public bool IsLocalDeleted { get; set; }

		public TagVisualElement() : base(nameof(TagVisualElement))
		{

		}

		public override void Refresh()
		{
			base.Refresh();

			_backGroundElement = Root.Q<VisualElement>("mainVisualElement");
			if (IsLocalOnly)
				_backGroundElement.AddToClassList("localOnly");
			else if (IsLocalDeleted)
				_backGroundElement.AddToClassList("localDeleted");
			else
			{
				_backGroundElement.RemoveFromClassList("localOnly");
				_backGroundElement.RemoveFromClassList("localDeleted");
			}

			_label = Root.Q<Label>("label");
			_label.text = Text;
		}
	}
}
