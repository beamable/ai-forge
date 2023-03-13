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
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.EventSystems;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
static class TestUtil
{
	public class TestEditorWindow : EditorWindow
	{
		public VisualElement Root => this.GetRootVisualContainer();
		public void Mount(VisualElement element)
		{
			;
			Root.Clear();
			Root.Add(element);
		}
	}

	/// <summary>
	/// The root visual element for your test needs to be mounted onto a real IMGUI window.
	/// Call this method with the root element for your test.
	/// Before sending any events, you'll need to yield a Unity render frame, so that Unity has time to force the layout
	/// </summary>
	/// <param name="elem"></param>
	/// <returns>An EditorWindow that you must explicitly call `Close()` on at the end of the test.</returns>
	public static TestEditorWindow MountForTest(this VisualElement elem)
	{
		var window = EditorWindow.GetWindow<TestEditorWindow>();
		window.Mount(elem);
		return window;
	}

	/// <summary>
	/// In order to trick a Button into receiving a click, we need to send both a MouseDown and MouseUp event. This utility method makes that a
	/// little easier.
	/// </summary>
	/// <param name="button"></param>
	public static void SendTestClick(this Button button)
	{
#if UNITY_2019 || UNITY_2020
		using (var evt = MouseDownEvent.GetPooled(button.worldBound.position + Vector2.one, 0, 1, Vector2.zero, EventModifiers.None))
		{
			button.SendEvent(evt);
		}
		using (var evt = MouseUpEvent.GetPooled(button.worldBound.position + Vector2.one, 0, 1, Vector2.zero, EventModifiers.None))
		{
			button.SendEvent(evt);
		}
#elif UNITY_2021_1_OR_NEWER
		using (var evt = new NavigationSubmitEvent() { target = button })
			button.SendEvent(evt);
#endif
	}

	/// <summary>
	/// In order to trick a TextField into receiving a keystroke, we need to send both a KeyDown and KeyUp event. This utility method makes that a
	/// little easier.
	/// </summary>
	/// <param name="textField"></param>
	public static void SendTestKeystroke(this TextField textField, string text)
	{

		textField.BeamableFocus();
		foreach (var letter in text)
		{
			var es = Event.KeyboardEvent(letter.ToString());

			es.character = letter;
			using (var evt = KeyDownEvent.GetPooled(es))
			{
				textField.SendEvent(evt);
			}

			using (var evt = KeyUpEvent.GetPooled(Event.KeyboardEvent(letter.ToString())))
			{
				textField.SendEvent(evt);
			}
		}

		textField.Blur();
	}
}
