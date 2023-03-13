using Beamable.Editor.Content.Components;
using NUnit.Framework;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Tests.UI.Content.Components.CountVisualElementTests
{
	public class BehaviourTests
	{

		[Test]
		public void ShowsValueInLabel()
		{
			var countElem = new CountVisualElement();
			countElem.Refresh();
			countElem.Value = 12;

			var lbl = countElem.Q<Label>();
			Assert.NotNull(lbl);
			Assert.AreEqual("12", lbl.text);
		}

		[Test]
		public void AddsDangerClassTag()
		{
			var countElem = new CountVisualElement();
			countElem.Refresh();
			countElem.Value = 2;
			Assert.IsFalse(countElem.ClassListContains("danger"));

			countElem.IsDangerous = true;

			var lbl = countElem.Q<Label>();
			Assert.NotNull(lbl);
			Assert.AreEqual("2", lbl.text);
			Assert.IsTrue(countElem.ClassListContains("danger"));
		}
	}
}
