using UnityEditor;
using UnityEngine;

namespace Beamable.Samples.SampleProjectBase
{
	/// <summary>
	/// Ping a custom-formatted readme file and force-show in inspector. Parse the
	/// custom format to markdown-like display.
	///
	/// Inspired by Unity's "Learn" Sample Projects
	///
	/// </summary>
	[CustomEditor(typeof(Readme))]
	[InitializeOnLoad]
	public class BeamableReadmeEditor : UnityEditor.Editor
	{
		static float kSpace = 16f;

		protected static Readme SelectReadme(string findAssetsFilter, string[] findAssetsFolders)
		{
			var ids = AssetDatabase.FindAssets(findAssetsFilter, findAssetsFolders);

			if (ids.Length == 1)
			{
				var pathToReadme = AssetDatabase.GUIDToAssetPath(ids[0]);
				return SelectReadme(pathToReadme);
			}
			else if (ids.Length > 1)
			{
				Debug.LogError("SelectReadme() Too many results found for Readme.");
			}
			else
			{
				Debug.LogError("SelectReadme() No results found for Readme.");
			}

			return null;
		}


		private static Readme SelectReadme(string pathToReadme)
		{
			if (string.IsNullOrEmpty(pathToReadme))
			{
				return null;
			}
			var readmeObject = AssetDatabase.LoadMainAssetAtPath(pathToReadme);

			if (readmeObject == null)
			{
				return null;
			}

			var editorAsm = typeof(UnityEditor.Editor).Assembly;
			var inspWndType = editorAsm.GetType("UnityEditor.InspectorWindow");
			var window = EditorWindow.GetWindow(inspWndType);
			window.Focus();

			Selection.objects = new UnityEngine.Object[] { readmeObject };
			return (Readme)readmeObject;
		}

		protected override void OnHeaderGUI()
		{
			var readme = (Readme)target;
			Init();

			var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

			GUILayout.BeginHorizontal("In BigTitle");
			{
				GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
				GUILayout.Label(readme.title, TitleStyle);
			}
			GUILayout.EndHorizontal();
		}

		public override void OnInspectorGUI()
		{

			var readme = (Readme)target;
			Init();

			foreach (var section in readme.sections)
			{
				if (!string.IsNullOrEmpty(section.heading))
				{
					GUILayout.Label(section.heading, HeadingStyle);
				}
				if (!string.IsNullOrEmpty(section.text))
				{
					GUILayout.Label(section.text, BodyStyle);
				}
				if (!string.IsNullOrEmpty(section.linkText))
				{
					if (LinkLabel(new GUIContent(section.linkText)))
					{
						Application.OpenURL(section.url);
					}
				}
				GUILayout.Space(kSpace);
			}
		}


		bool m_Initialized;

		GUIStyle LinkStyle { get { return m_LinkStyle; } }
		[SerializeField] GUIStyle m_LinkStyle;

		GUIStyle TitleStyle { get { return m_TitleStyle; } }
		[SerializeField] GUIStyle m_TitleStyle;

		GUIStyle HeadingStyle { get { return m_HeadingStyle; } }
		[SerializeField] GUIStyle m_HeadingStyle;

		GUIStyle BodyStyle { get { return m_BodyStyle; } }
		[SerializeField] GUIStyle m_BodyStyle;

		void Init()
		{
			if (m_Initialized)
				return;
			m_BodyStyle = new GUIStyle(EditorStyles.label);
			m_BodyStyle.wordWrap = true;
			m_BodyStyle.fontSize = 14;

			m_TitleStyle = new GUIStyle(m_BodyStyle);
			m_TitleStyle.fontSize = 26;

			m_HeadingStyle = new GUIStyle(m_BodyStyle);
			m_HeadingStyle.fontSize = 18;

			m_LinkStyle = new GUIStyle(m_BodyStyle);
			m_LinkStyle.wordWrap = false;
			// Match selection color which works nicely for both light and dark skins
			m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
			m_LinkStyle.stretchWidth = false;

			m_Initialized = true;
		}

		bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
		{
			var position = GUILayoutUtility.GetRect(label, LinkStyle, options);

			Handles.BeginGUI();
			Handles.color = LinkStyle.normal.textColor;
			Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
			Handles.color = Color.white;
			Handles.EndGUI();

			EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

			return GUI.Button(position, label, LinkStyle);
		}
	}
}
