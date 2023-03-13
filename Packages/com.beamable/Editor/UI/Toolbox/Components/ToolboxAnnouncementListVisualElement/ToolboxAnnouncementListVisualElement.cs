using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.Toolbox.UI.Components;
using Beamable.Editor.UI.Components;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Toolbox.Components
{
	public class ToolboxAnnouncementListVisualElement : ToolboxComponent
	{
		private VisualElement _mainContainer;
		public event Action<float> OnHeightChanged;

		public new class UxmlFactory : UxmlFactory<ToolboxAnnouncementListVisualElement, UxmlTraits>
		{
		}
		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as ToolboxAnnouncementListVisualElement;

			}
		}

		public IToolboxViewService Model { get; set; }

		public ToolboxAnnouncementListVisualElement() : base(nameof(ToolboxAnnouncementListVisualElement))
		{

		}

		public override void Refresh()
		{
			base.Refresh();

			_mainContainer = Root.Q<VisualElement>("main");

			Model = Provider.GetService<IToolboxViewService>();

			SetAnnouncements(Model.Announcements);
			Model.OnAnnouncementsChanged += SetAnnouncements;
		}

		private void SetAnnouncements(IEnumerable<AnnouncementModelBase> announcements)
		{
			var startHeight = _mainContainer.worldBound.height;

			_mainContainer.Clear();

			foreach (var announcement in announcements)
			{
				BeamableVisualElement elem = announcement.CreateVisualElement();
				_mainContainer.Add(elem);
				elem.Refresh();
			}

			var attemptsLeft = 250;
			void WaitForRedraw()
			{
				var height = _mainContainer.worldBound.height;
				if (attemptsLeft-- > 0 && (Mathf.Abs(height - startHeight) < 1) || float.IsNaN(height))
				{
					EditorApplication.delayCall += WaitForRedraw;
				}
				else
				{
					// its time.
					OnHeightChanged?.Invoke(height);
				}
			}

			EditorApplication.delayCall += WaitForRedraw;
		}
	}
}
