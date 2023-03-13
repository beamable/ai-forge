using Beamable.Common.Content;
using Beamable.Common.Content.Serialization;
using Beamable.Common.Content.Validation;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beamable.Editor.Content.UI
{
#if !BEAMABLE_NO_CONTENT_INSPECTOR
	[CanEditMultipleObjects]
	[CustomEditor(typeof(ContentObject), true)]
	public class ContentObjectEditor : UnityEditor.Editor
	{

		protected override void OnHeaderGUI()
		{
			base.OnHeaderGUI();

			var leftMargin = 45;
			var rightMargin = 56;
			Rect lastRect = GUILayoutUtility.GetLastRect();
			Rect r = new Rect(lastRect.x + leftMargin, lastRect.height - 25, lastRect.width - (leftMargin + rightMargin),
			   20);


			var contentObject = target as ContentObject;
			if (contentObject == null) return;

			if (contentObject.ContentName == null)
				contentObject.SetContentName(contentObject.name);

			if (ContentNameValidationException.HasNameValidationErrors(contentObject, contentObject.ContentName, out var nameErrors))
			{
				var errorText = string.Join(",", nameErrors.Select(n => n.Message));
				var idValidationRect = new Rect(lastRect.x, lastRect.y, 4, lastRect.height);
				EditorGUI.DrawRect(idValidationRect, Color.red);

				var redStyle = new GUIStyle(GUI.skin.label);
				redStyle.normal.textColor = Color.red;
				redStyle.fontSize = 8;
				EditorGUI.LabelField(new Rect(lastRect.x + leftMargin, lastRect.y, lastRect.width - leftMargin, 12), $"({errorText})", redStyle);

			}

			EditorGUI.BeginChangeCheck();

			var oldFieldWith = EditorGUIUtility.labelWidth;

			var value = GetTagString(contentObject.Tags);
			if (targets.Length > 1)
			{
				for (var i = 0; i < targets.Length; i++)
				{
					var otherContentObject = targets[i] as ContentObject;
					if (otherContentObject == null) continue;
					var otherValue = GetTagString(otherContentObject.Tags);
					if (otherValue != value)
					{
						value = "-";
						break;
					}
				}
			}

			EditorGUIUtility.labelWidth = 75;
			var edit = EditorGUI.TextField(r, "Content Tag", value);
			EditorGUIUtility.labelWidth = oldFieldWith;

			if (EditorGUI.EndChangeCheck())
			{
				SetTagAt = EditorApplication.timeSinceStartup + .25; // debounce time
				latestDebounceId++;
				var debounceId = latestDebounceId;
				EditorApplication.delayCall += () => DebounceTagSet(debounceId, edit);

			}
		}

		private double SetTagAt = 0;
		private long latestDebounceId = 0;
		void DebounceTagSet(long id, string edit)
		{
			if (id != latestDebounceId) return; // a more recent debounce is scheduled. this one can die.

			if (EditorApplication.timeSinceStartup < SetTagAt)
			{
				EditorApplication.delayCall += () => DebounceTagSet(id, edit); // try again.
				return;
			}

			var tags = GetTagsFromString(edit);
			Undo.RecordObjects(targets, "Change Content Tag");
			foreach (Object obj in targets)
			{
				var otherContentObject = obj as ContentObject;
				if (otherContentObject != null)
				{
					otherContentObject.Tags = tags.ToArray(); // copy.

					otherContentObject.ForceValidate();
				}
			}
		}


		public override void OnInspectorGUI()
		{

			var contentObject = target as ContentObject;
			if (contentObject == null) return;

			if (contentObject.SerializeToConsoleRequested)
			{
				contentObject.SerializeToConsoleRequested = false;
				var serialized = ClientContentSerializer.SerializeContent(contentObject);
				Debug.Log(serialized);
			}

			if (ContentObject.ShowChecksum)
			{
				var checksumStyle = new GUIStyle(GUI.skin.label);
				checksumStyle.fontSize = 10;
				// TODO: Can we get a fixed width
				checksumStyle.alignment = TextAnchor.MiddleRight;

				float contextWidth = (float)typeof(EditorGUIUtility)
				   .GetProperty("contextWidth", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null, null);

				var rect = EditorGUILayout.GetControlRect(false, 12, checksumStyle);

				rect = new Rect(rect.x, rect.y - 6, rect.width, rect.height);
				GUI.Box(new Rect(0, rect.y, contextWidth, rect.height), "", "In BigTitle Post");

				EditorGUI.SelectableLabel(rect, $"Checksum: {ContentIO.ComputeChecksum(contentObject)}", checksumStyle);
			}

			base.OnInspectorGUI();
		}

		public string GetTagString(string[] tags)
		{
			return string.Join(" ", tags);
		}

		public string[] GetTagsFromString(string tagString)
		{
			return tagString?.Split(new[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
		}
	}
#endif

}
