using Beamable.Common;
using Beamable.Editor.Toolbox.UI.Components;
using Beamable.Editor.UI.Components;
using System.Collections;
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

namespace Beamable.Editor.Toolbox.Models
{
	public interface IWebsiteHook
	{
		string Url { get; }
		void OpenUrl(string url);
	}

	public class WebsiteHook : IWebsiteHook
	{
		public string Url { get; private set; }
		public void OpenUrl(string url)
		{
			Url = url;
			Application.OpenURL(url);
		}
	}
}
