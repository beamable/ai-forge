using Beamable.Common;
using Beamable.Editor.Toolbox.UI.Components;
using Beamable.Editor.UI.Components;
using Beamable.Editor.Toolbox.Models;
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
using static Beamable.Common.Constants;
using static Beamable.Common.Constants.MenuItems.Windows;

namespace Beamable.Editor.Toolbox.Components
{
	public class ToolboxBreadcrumbsVisualElement
		: ToolboxComponent
	{
		public new class UxmlFactory : UxmlFactory<ToolboxBreadcrumbsVisualElement, UxmlTraits>
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
				var self = ve as ToolboxBreadcrumbsVisualElement;
			}
		}
		private Button _accountButton;
		private RealmButtonVisualElement _realmButton;
		private IWebsiteHook WebsiteHook { get; set; }

		public ToolboxBreadcrumbsVisualElement() : base(nameof(ToolboxBreadcrumbsVisualElement))
		{
		}

		public override void Refresh()
		{
			base.Refresh();
			_realmButton = Root.Q<RealmButtonVisualElement>("realmButton");
			_realmButton.Refresh();

			WebsiteHook = Provider.GetService<IWebsiteHook>();

			var portalButton = Root.Q<Button>("openPortalButton");
			portalButton.text = (Commons.OPEN + " " + Names.PORTAL).ToUpper();
			portalButton.clickable.clicked += () => GetPortalUrl.Then(WebsiteHook.OpenUrl);
			var m = new ContextualMenuManipulator(rightClickEvt =>
			{
				rightClickEvt.menu.BeamableAppendAction("Copy Url",
					mp => { GetPortalUrl.Then(url => { EditorGUIUtility.systemCopyBuffer = url; }); });
			})
			{
				target = portalButton
			}
				;

		}


		private Promise<string> GetPortalUrl
		{
			get
			{
				var de = Context;
				var url = $"{BeamableEnvironment.PortalUrl}/{de.CurrentCustomer.Cid}/games/{de.ProductionRealm.Pid}/realms/{de.CurrentRealm.Pid}/dashboard?refresh_token={de.Requester.Token.RefreshToken}";
				return Promise<string>.Successful(url);
			}
		}
	}
}
