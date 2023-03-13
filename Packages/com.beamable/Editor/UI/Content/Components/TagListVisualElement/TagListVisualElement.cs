using Beamable.Editor.Content.Models;
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
	public class TagListVisualElement : ContentManagerComponent
	{

		public new class UxmlFactory : UxmlFactory<TagListVisualElement, UxmlTraits> { }
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
				var self = ve as TagListVisualElement;
			}
		}

		private VisualElement _mainVisualElement;

		public List<ContentTagDescriptor> TagDescriptors { get; set; }

		public TagListVisualElement() : base(nameof(TagListVisualElement))
		{

		}

		public override void Refresh()
		{
			base.Refresh();

			_mainVisualElement = Root.Q<VisualElement>("mainVisualElement");

			foreach (var tagDescriptor in TagDescriptors)
			{
				AddTagVisualElement(tagDescriptor.Tag,
				   tagDescriptor.LocalStatus == HostStatus.AVAILABLE && tagDescriptor.ServerStatus != HostStatus.AVAILABLE,
				   tagDescriptor.LocalStatus == HostStatus.NOT_AVAILABLE && tagDescriptor.ServerStatus == HostStatus.AVAILABLE);
			}
		}

		private void AddTagVisualElement(string tag, bool localOnly, bool localDeleted)
		{
			TagVisualElement tagVisualElement = new TagVisualElement();
			tagVisualElement.Text = tag;
			tagVisualElement.IsLocalOnly = localOnly;
			tagVisualElement.IsLocalDeleted = localDeleted;
			tagVisualElement.Refresh();
			_mainVisualElement.Add(tagVisualElement);
		}
	}
}
