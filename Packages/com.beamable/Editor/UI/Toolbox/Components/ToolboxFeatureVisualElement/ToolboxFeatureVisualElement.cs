using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.Toolbox.UI.Components;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Toolbox.Components
{
	public class ToolboxFeatureVisualElement : ToolboxComponent
	{
		public new class UxmlFactory : UxmlFactory<ToolboxContentListVisualElement, UxmlTraits>
		{
		}

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlStringAttributeDescription customText = new UxmlStringAttributeDescription
			{ name = "custom-text", defaultValue = "nada" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as ToolboxContentListVisualElement;
			}
		}

		public Widget WidgetModel { get; set; }

		public ToolboxFeatureVisualElement() : base(nameof(ToolboxFeatureVisualElement))
		{
		}

		public override void Refresh()
		{
			base.Refresh();
			if (WidgetModel == null) return;

			Label titlelabel = Root.Q<Label>("title");
			titlelabel.text = WidgetModel.Name;

			Label despLabel = Root.Q<Label>("description");
			despLabel.text = WidgetModel.Description;
			despLabel.AddTextWrapStyle();

			Image functionImage = Root.Q<Image>("functionIcon");
			functionImage.image = WidgetModel.Icon;

			var contentNode = Root.Q(className: "mainContent");
			//contentNode.AddToClassList("highLight");

			Root.RegisterCallback<MouseOverEvent, VisualElement>((evt, widgetNode) =>
			{
				if (isButtonDown) return;
				var imageMask = widgetNode.Q("imageMask");
				imageMask.RemoveFromClassList("imageMaskDefault");
				imageMask.RemoveFromClassList("imageMaskSelected");
				imageMask.AddToClassList("imageMaskHover");

			}, Root);
			Root.RegisterCallback<MouseDownEvent, VisualElement>((evt, widgetNode) =>
			{

				var imageMask = widgetNode.Q("imageMask");
				imageMask.RemoveFromClassList("imageMaskDefault");
				imageMask.RemoveFromClassList("imageMaskHover");
				imageMask.AddToClassList("imageMaskSelected");
				isButtonDown = true;

				DragDFab(evt, widgetNode);

			}, Root);

			Root.RegisterCallback<MouseLeaveEvent, VisualElement>((evt, widgetNode) =>
			{
				isButtonDown = false;
				SetImageMaskToDefault(widgetNode);
			}, Root);
			Root.RegisterCallback<MouseUpEvent, VisualElement>((evt, widgetNode) =>
			{
				isButtonDown = false;
				SetImageMaskToDefault(widgetNode);
			}, Root);
			var lastUpdatedTime = 0.0;
			var checkNumber = 0;
			void Check(int id)
			{
				if (id != checkNumber) return;
				if (EditorApplication.timeSinceStartup > lastUpdatedTime + .2)
				{
					// Clean up, phase.
					isButtonDown = false;
					SetImageMaskToDefault(Root);
					checkNumber = 0;
				}
				else if (id == checkNumber)
				{
					EditorApplication.delayCall += () => Check(id);
				}
			}
			Root.RegisterCallback<DragUpdatedEvent>(evt =>
			{
				lastUpdatedTime = EditorApplication.timeSinceStartup;

				EditorApplication.delayCall += () => Check(++checkNumber);
			});

			Root.RegisterCallback<DragExitedEvent, VisualElement>((evt, widgetNode) =>
			{
				isButtonDown = false;
				SetImageMaskToDefault(widgetNode);
			}, Root);


		}

		// private VisualElement imageMask;
		private static bool isButtonDown = false;

		void SetImageMaskToDefault(VisualElement widgetNode)
		{
			var imageMask = widgetNode.Q("imageMask");
			imageMask.RemoveFromClassList("imageMaskSelected");
			imageMask.RemoveFromClassList("imageMaskHover");
			imageMask.AddToClassList("imageMaskDefault");
		}

		void DragDFab(IMouseEvent evt, VisualElement widgetNode)
		{
			DragAndDrop.PrepareStartDrag();

			DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

			DragAndDrop.objectReferences = new Object[] { WidgetModel.Prefab };

			DragAndDrop.StartDrag($"Create {WidgetModel.Name}");

		}

	}
}
