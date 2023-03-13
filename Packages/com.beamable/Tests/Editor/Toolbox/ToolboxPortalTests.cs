using Beamable.Common.Dependencies;
using Beamable.Editor.Toolbox.Components;
using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.UI;
using Beamable.Editor.UI.Components;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Events;
using UnityEditor.EventSystems;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Beamable.Editor.Tests.Toolbox
{
	public class ToolboxPortalTests : EditorTest
	{
		protected override void Configure(IDependencyBuilder builder)
		{
			builder.ReplaceSingleton<IWebsiteHook, MockWebsiteHook>();
		}

		// A Test behaves as an ordinary method
		[UnityTest]
		public IEnumerator PortalButtonTest()
		{
			IWebsiteHook websiteHook = Provider.GetService<IWebsiteHook>();

			ToolboxBreadcrumbsVisualElement tbBreadcrumbs = new ToolboxBreadcrumbsVisualElement();
			tbBreadcrumbs.Refresh(Provider);

			var portalButton = tbBreadcrumbs.Q<Button>("openPortalButton");

			var window = portalButton.MountForTest();

			yield return null;

			portalButton.SendTestClick();
			window.Close();

			Debug.Log(websiteHook.Url);

			var de = Context;
			string url = $"{BeamableEnvironment.PortalUrl}/{de.CurrentCustomer.Cid}/games/{de.ProductionRealm.Pid}/realms/{de.CurrentRealm.Pid}/dashboard?refresh_token={de.Requester.Token.RefreshToken}";

			Assert.AreEqual(url, websiteHook.Url);
		}
	}
}
