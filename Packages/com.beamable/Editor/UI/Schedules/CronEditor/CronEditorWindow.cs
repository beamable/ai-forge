using Beamable.Editor.UI;
using System;
using UnityEngine;

namespace Beamable.Editor.Content
{
	public class CronEditorWindow : WindowBase<CronEditorWindow, CronEditorVisualElement>
	{
		private string _cronRawFormat;
		private Action<string> _result;

		public void Init(string cronRawFormat, Action<string> result)
		{
			_cronRawFormat = cronRawFormat;
			_result = result;

			titleContent = new GUIContent("Cron Editor");
			minSize = new Vector2(400f, 300f);

			Refresh();
		}

		protected override CronEditorVisualElement GetVisualElement() => new CronEditorVisualElement(_cronRawFormat, _result);
	}
}
