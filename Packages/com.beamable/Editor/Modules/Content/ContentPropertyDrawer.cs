using Beamable.Common.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beamable.Editor.Content
{

#if !BEAMABLE_NO_REF_DRAWERS
	[CustomPropertyDrawer(typeof(BaseContentRef), true)]
#endif
	public class ContentRefPropertyDrawer : PropertyDrawer
	{
		private static BeamEditorContext _beamable;
		private static bool _requestedBeamable;
		private static Dictionary<Type, HashSet<string>> _typeToContent = new Dictionary<Type, HashSet<string>>();
		private static Dictionary<Type, double> _typeToRefreshAt = new Dictionary<Type, double>();

		public static void ResetContent()
		{
			_typeToContent = new Dictionary<Type, HashSet<string>>();
		}
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);
			var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
			var fieldValue = GetTargetObjectOfProperty(property) as BaseContentRef;

			if (fieldValue == null) return;

			label.tooltip = PropertyDrawerHelper.SetTooltipWithFallback(fieldInfo, property);

			var idVal = fieldValue.GetId();
			idVal = string.IsNullOrEmpty(idVal)
			   ? "<none>"
			   : idVal;

			var referenceType = fieldValue.GetReferencedBaseType();
			var time = EditorApplication.timeSinceStartup;
			if (_beamable == null)
			{
				if (!_requestedBeamable)
				{
					_requestedBeamable = true;
					var beamable = BeamEditorContext.Default;
					_beamable = beamable;
				}
			}
			else if (!_typeToContent.ContainsKey(referenceType) || time > _typeToRefreshAt[referenceType])
			{
				_typeToRefreshAt[referenceType] = time + 5; // every five seconds; rescan

				_beamable.ContentIO.EnsureDefaultContentByType(referenceType);
				var allContent =
				   new HashSet<string>(_beamable.ContentDatabase.GetContent(referenceType).Select(x => x.contentId));
				_typeToContent[referenceType] = allContent;
			}

			if (_typeToContent.ContainsKey(referenceType) && !_typeToContent[referenceType].Contains(idVal) && !idVal.Contains("(missing)"))
			{
				idVal += " (missing)";
			}

			EditorGUI.PrefixLabel(labelRect, label);
			var buttonClick = EditorGUI.DropdownButton(fieldRect, new GUIContent(idVal, label.tooltip), FocusType.Keyboard);
			if (buttonClick)
			{
				var wnd = ScriptableObject.CreateInstance<ContentRefSearchWindow>();
				wnd.Property = property;
				wnd.FieldInfo = fieldInfo;
				wnd.FieldValue = fieldValue;
				wnd.Label = label;
				wnd.Object = property.serializedObject.targetObject;

				wnd.Init();
				var xy = EditorGUIUtility.GUIToScreenPoint(new Vector2(fieldRect.x, fieldRect.y));
				wnd.ShowAsDropDown(new Rect((int)xy.x, (int)xy.y + fieldRect.height, 0, 0),
				   new Vector2(fieldRect.width, 300));
			}
		}
		public static object GetTargetObjectOfProperty(SerializedProperty prop)
		   => GetTargetObjectOfProperty(prop, prop.serializedObject.targetObject);
		private static object GetTargetObjectOfProperty(SerializedProperty prop, object obj)
		{
			if (prop == null) return null;
			var path = prop.propertyPath.Replace(".Array.data[", "[");
			var elements = path.Split('.');

			foreach (var element in elements)
			{
				if (element.Contains("["))
				{
					var elementName = element.Substring(0, element.IndexOf("["));
					var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "")
					   .Replace("]", ""));
					obj = GetValue_Imp(obj, elementName, index);
				}
				else
				{
					obj = GetValue_Imp(obj, element);
				}
			}

			return obj;
		}
		public static IEnumerable<object> GetTargetObjectsOfProperty(SerializedProperty prop)
		{
			var targetObjectsOfProperty = new object[prop.serializedObject.targetObjects.Length];
			for (var i = 0; i < prop.serializedObject.targetObjects.Length; i++)
			{
				var obj = GetTargetObjectOfProperty(prop, prop.serializedObject.targetObjects[i]);
				if (obj != null)
				{
					targetObjectsOfProperty[i] = obj;
				}
			}
			return targetObjectsOfProperty;
		}
		public static object GetTargetParentObjectOfProperty(SerializedProperty prop, int parentsUp = 1)
		{
			if (prop == null) return null;

			var path = prop.propertyPath.Replace(".Array.data[", "[");
			object obj = prop.serializedObject.targetObject;
			var elements = path.Split('.');
			elements = elements.Take(elements.Length - parentsUp).ToArray();
			foreach (var element in elements)
			{
				if (element.Contains("["))
				{
					var elementName = element.Substring(0, element.IndexOf("["));
					var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "")
					   .Replace("]", ""));
					obj = GetValue_Imp(obj, elementName, index);
				}
				else
				{
					obj = GetValue_Imp(obj, element);
				}
			}

			return obj;
		}

		private static object GetValue_Imp(object source, string name)
		{
			if (source == null)
				return null;
			var type = source.GetType();

			while (type != null)
			{
				var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				if (f != null)
					return f.GetValue(source);

				var p = type.GetProperty(name,
				   BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (p != null)
					return p.GetValue(source, null);

				type = type.BaseType;
			}

			return null;
		}
		private static object GetValue_Imp(object source, string name, int index)
		{
			var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
			if (enumerable == null) return null;
			var enm = enumerable.GetEnumerator();
			//while (index-- >= 0)
			//    enm.MoveNext();
			//return enm.Current;

			for (int i = 0; i <= index; i++)
			{
				if (!enm.MoveNext()) return null;
			}

			return enm.Current;
		}
	}

	public class ContentRefSearchWindow : EditorWindow
	{
		struct Option
		{
			public string DisplayName;
			public string DisplayNameLower;
			public string Id;
		}

		public BaseContentRef FieldValue { get; set; }
		public SerializedProperty Property { get; set; }
		public FieldInfo FieldInfo { get; set; }
		public Object Object { get; set; }
		public GUIContent Label { get; set; }

		private Vector2 _scrollPos;
		private string _searchString;
		private bool _initialized;
		private string _typeName;
		private GUIStyle _normalStyle, _activeStyle;
		private List<Option> _options;
		private Dictionary<string, Option> _idToOption;
		private int _selectedIndex;
		private Texture2D _highlightTexture;
		private Texture2D _activeTexture;

		private const string SearchControlName = "contentRefSearchBar";

		public async void Init()
		{
			var referenceType = FieldValue.GetReferencedBaseType();
			var contentTypeReflectionCache = BeamEditor.GetReflectionSystem<ContentTypeReflectionCache>();
			_typeName = contentTypeReflectionCache.TypeToName(referenceType);
			var de = BeamEditorContext.Default;
			await de.InitializePromise;

			de.ContentIO.EnsureDefaultContentByType(referenceType);
			var contentEntries = de.ContentDatabase.GetContent(referenceType);
			_options = new List<Option>();
			_idToOption = new Dictionary<string, Option>();
			foreach (var content in contentEntries)
			{
				var displayName = content.contentId.Substring(_typeName.Length + 1);
				var option = new Option
				{
					Id = content.contentId,
					DisplayName = displayName,
					DisplayNameLower = displayName.ToLower()
				};
				_options.Add(option);
				_idToOption.Add(content.contentId, option);
			}

			var nullOption = new Option
			{
				DisplayName = "<none>",
				DisplayNameLower = "none",
				Id = null
			};
			_options.Insert(0, nullOption);
			_idToOption.Add("", nullOption);


			var currId = FieldValue.GetId();
			_selectedIndex = 0;
			if (!string.IsNullOrEmpty(currId))
			{
				_selectedIndex = _options.FindIndex(o => currId.Equals(o.Id));
			}

			_highlightTexture = MakeTex(1, 1, new Color(0, 0, 0, .1f));
			_activeTexture = MakeTex(1, 1, new Color(0, .5f, 1, .2f));

			_activeStyle = new GUIStyle(GUI.skin.label)
			{
				normal = { background = _activeTexture },
				hover = { background = _highlightTexture },
				wordWrap = true
			};

			_normalStyle = new GUIStyle(GUI.skin.label)
			{
				hover = { background = _highlightTexture },
				wordWrap = true
			};
			_initialized = true;

		}
		private Texture2D MakeTex(int width, int height, Color col)
		{
			Color[] pix = new Color[width * height];

			for (int i = 0; i < pix.Length; i++)
				pix[i] = col;

			Texture2D result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();

			return result;
		}
		protected void OnGUI()
		{
			if (!_initialized)
			{
				EditorGUILayout.PrefixLabel("...fetching");
				return;
			}

			GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"), GUILayout.Height(30));

			GUI.SetNextControlName(SearchControlName);
			_searchString = GUILayout.TextField(_searchString, GUI.skin.FindStyle("ToolbarSeachTextField")); // SIC. The "ToolbarSeachTextField" is on purpose. It's a Unity typo.
			GUI.FocusControl(SearchControlName);

			if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
			{
				// Remove focus if cleared
				_searchString = "";
				GUI.FocusControl(null);
			}

			GUILayout.EndHorizontal();

			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(this.position.height - 30));
			var searchLower = _searchString?.ToLower() ?? "";

			var filteredOptions = _options.Where(option =>
			   string.IsNullOrEmpty(_searchString) || option.DisplayNameLower.Contains(searchLower)).ToArray();
			var contents = filteredOptions.Select(option => new GUIContent(option.DisplayName)).ToList();

			var index = -1;

			EditorGUI.BeginChangeCheck();

			for (var i = 0; i < contents.Count; i++)
			{
				var c = contents[i];
				var isSelected = _selectedIndex == i;
				var style = isSelected ? _activeStyle : _normalStyle;

				var clicked = GUILayout.Button(c, style, GUILayout.MaxWidth(position.width - 20));
				if (clicked)
				{
					index = i;
				}
			}

			EditorGUILayout.EndScrollView();

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(Object, "Change Content Ref");
				var selected = filteredOptions[index];
				var fieldValues = ContentRefPropertyDrawer.GetTargetObjectsOfProperty(Property);
				foreach (BaseContentRef fieldValue in fieldValues)
				{
					fieldValue.SetId(selected.Id);
				}

				Property.serializedObject.Update();
				EditorUtility.SetDirty(Object);

				foreach (var targetObject in Property.serializedObject.targetObjects)
				{
					if (targetObject is ContentObject contentObject)
					{
						contentObject.ForceValidate();
					}
				}

				Close();
			}

			Property.serializedObject.UpdateIfRequiredOrScript();
			Repaint(); // make gui reactive
		}
	}

}
