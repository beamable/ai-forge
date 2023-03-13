using Beamable.Common.Dependencies;
using Beamable.Editor.Toolbox.Components;
using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.UI;
using Beamable.Editor.UI.Components;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Beamable.Editor.Tests.Toolbox
{
	public class ToolboxTextboxTests : EditorTest
	{
		protected override void Configure(IDependencyBuilder builder)
		{
			builder.ReplaceSingleton<IToolboxViewService, MockToolboxViewService>();
		}

		[UnityTest]
		public IEnumerator TextboxKeystrokeTest()
		{
			IToolboxViewService model = Provider.GetService<IToolboxViewService>();

			ToolboxActionBarVisualElement tbActionBar = new ToolboxActionBarVisualElement();
			tbActionBar.Refresh(Provider);

			var search = tbActionBar.Q<SearchBarVisualElement>();
			TextField text = search.Q<TextField>();

			var window = tbActionBar.MountForTest();

			yield return null;
			text.SendTestKeystroke("TESTing");

			yield return new WaitForSecondsRealtime(.5f);
			window.Close();

			Debug.Log(text.value);
			Assert.AreEqual("TESTing", model.Query.ToString());

			model.SetQuery(string.Empty);
		}
	}
}
