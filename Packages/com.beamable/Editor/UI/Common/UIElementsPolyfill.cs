using Beamable.Common;
using Beamable.Editor.UI.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements.StyleSheets;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

#if UNITY_2018
namespace UnityEngine.Experimental.UIElements
{
	using StyleSheets;

	public static class UIElementsPolyfill2018
	{
		public static void AddSplitPane(this VisualElement self, VisualElement left, VisualElement right)
		{
			var splitterElem = new SplitterVisualElement() {name = "splitter"};

			var leftWrapper = new VisualElement();
			leftWrapper.AddToClassList("splitWrapper");
			leftWrapper.AddToClassList("leftSplit");
			var rightWrapper = new VisualElement();
			rightWrapper.AddToClassList("splitWrapper");
			rightWrapper.AddToClassList("rightSplit");
			leftWrapper.Add(left);
			rightWrapper.Add(right);

			splitterElem.Add(leftWrapper);
			splitterElem.Add(rightWrapper);

			self.Add(splitterElem);
		}

		public static VisualElement AddTextWrapStyle(this VisualElement self)
		{
			self.style.wordWrap = true;
			return self;
		}

		public static VisualElement SetBackgroundScaleModeToFit(this VisualElement self)
		{
			self.style.backgroundScaleMode = ScaleMode.ScaleToFit;
			return self;
		}

		public static TextField BeamableReadOnly(this TextField self)
		{
			self.SetEnabled(false);
			self.style.opacity = 1;
			return self;
		}

		public static VisualElement CloneTree(this VisualTreeAsset self)
		{
			return self.CloneTree(null);
		}

		public static void AddStyleSheet(this VisualElement self, string path)
		{
			var paths = UssLoader.GetAvailableSheetPaths(path);
			foreach (var ussPath in paths)
			{
				self.AddStyleSheetPath(ussPath);
			}
		}

		public static void RemoveStyleSheet(this VisualElement self, string path)
		{
			self.RemoveStyleSheetPath(path);
		}

		public static void SetRight(this IStyle self, float value)
		{
			self.positionRight = value;
		}

		public static void SetLeft(this IStyle self, float value)
		{
			self.positionLeft = value;
		}

		public static void SetMarginLeft(this IStyle self, float value)
		{
			self.marginLeft = value;
		}

		public static float GetLeft(this VisualElement self)
		{
			return self.style.paddingLeft;
		}

		public static void SetTop(this IStyle self, float value)
		{
			self.positionTop = value;
		}

		public static void SetBottom(this IStyle self, float value)
		{
			self.positionBottom = value;
		}

		public static void SetFontSize(this IStyle self, float value)
		{
			self.fontSize = (int)value;
		}

		public static float GetWidth(this IStyle style)
		{
			return style.width.value;
		}

		public static void SetWidth(this IStyle self, float value)
		{
			self.width = value;
		}

		public static void SetHeight(this IStyle self, float value, bool overrideMaxHeight = false)
		{
			self.height = value;

			if (overrideMaxHeight)
				self.maxHeight = value;
		}

		public static float GetMaxHeight(this IStyle self)
		{
			return self.maxHeight;
		}

		public static void SetFlexDirection(this IStyle self, FlexDirection direction)
		{
			self.flexDirection = direction;
		}

		public static void SetFlexGrow(this IStyle self, float value){
			self.flexGrow = StyleValue<float>.Create(value);
		}

		public static void SetImage(this Image self, Texture texture)
		{
			self.image = StyleValue<Texture>.Create(texture);
		}

		public static void BeamableFocus(this TextField self)
		{
			self.Focus();
		}

		public static void SetFontStyle(this Label self, FontStyle style)
		{
			self.style.fontStyleAndWeight = style;
		}

		public static void BeamableAppendAction(this DropdownMenu self,
		                                        string title,
		                                        Action<Vector2> callback,
		                                        bool enabled = true)
		{
			if (enabled)
				self.AppendAction(title, evt => callback(evt.eventInfo.mousePosition),
				                  DropdownMenu.MenuAction.AlwaysEnabled);
			else
				self.AppendAction(title, evt => callback(evt.eventInfo.mousePosition),
				                  DropdownMenu.MenuAction.AlwaysDisabled);
		}

		public static bool RegisterValueChangedCallback<T>(this INotifyValueChanged<T> control,
		                                                   EventCallback<ChangeEvent<T>> callback)
		{
			CallbackEventHandler callbackEventHandler = control as CallbackEventHandler;
			if (callbackEventHandler == null)
				return false;
			callbackEventHandler.RegisterCallback<ChangeEvent<T>>(callback, TrickleDown.NoTrickleDown);
			return true;
		}

		public static bool UnregisterValueChangedCallback<T>(this INotifyValueChanged<T> control,
		                                                     EventCallback<ChangeEvent<T>> callback)
		{
			CallbackEventHandler callbackEventHandler = control as CallbackEventHandler;
			if (callbackEventHandler == null)
				return false;
			callbackEventHandler.UnregisterCallback<ChangeEvent<T>>(callback, TrickleDown.NoTrickleDown);
			return true;
		}

		public static VisualElement WithName(this VisualElement el, string name)
		{
			el.name = name;
			return el;
		}
		
		public static TextElement WithName(this TextElement el, string name)
		{
			el.name = name;
			return el;
		}
		
		public static ScrollView WithName(this ScrollView el, string name)
		{
			el.name = name;
			return el;
		}
		
		public static Label WithName(this Label el, string name)
		{
			el.name = name;
			return el;
		}
	}
}
#endif

#if UNITY_2019_1_OR_NEWER
namespace UnityEditor
{
  public static class UnityEditorPolyfill
  {
    public static VisualElement GetRootVisualContainer(this EditorWindow self)
    {
      return self.rootVisualElement;
    }
  }
}

namespace UnityEngine.UIElements
{

  public static class UIElementsPolyfill2019
  {

    public static void AddSplitPane(this VisualElement self, VisualElement left, VisualElement right) {

      var splitterElem = new SplitterVisualElement(){name = "splitter"};

      var leftWrapper = new VisualElement();
      leftWrapper.AddToClassList("splitWrapper");
      leftWrapper.AddToClassList("leftSplit");
      var rightWrapper = new VisualElement();
      rightWrapper.AddToClassList("splitWrapper");
      rightWrapper.AddToClassList("rightSplit");
      leftWrapper.Add(left);
      rightWrapper.Add(right);

      splitterElem.Add(leftWrapper);
      splitterElem.Add(rightWrapper);

      self.Add(splitterElem);

//      self.Add(left);
//      self.Add(right);


    }



    public static VisualElement AddTextWrapStyle(this VisualElement self)
    {
      self.style.whiteSpace = WhiteSpace.Normal;
      return self;
    }

    public static VisualElement SetBackgroundScaleModeToFit(this VisualElement self)
    {
      self.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
      return self;
    }

    public static TextField BeamableReadOnly(this TextField self)
    {
      // self.isReadOnly = true;
      self.SetEnabled(false);
      self.style.opacity = 1;
      return self;

    }

    public static void AddStyleSheet(this VisualElement self, string path)
    {
      var paths = UssLoader.GetAvailableSheetPaths(path);
      foreach (var ussPath in paths)
      {
        var sheetAsset = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
        if (sheetAsset == null)
        {
          Debug.LogWarning("Failed to load " + path + " for " + self?.name);
          continue;
        }
        self.styleSheets.Add(sheetAsset);
      }
    }

    public static void RemoveStyleSheet(this VisualElement self, string path)
    {
      self.styleSheets.Remove(AssetDatabase.LoadAssetAtPath<StyleSheet>(path));
    }
    public static void SetRight(this IStyle self, float value)
    {
      self.right = new StyleLength(value);
    }
    public static void SetTop(this IStyle self, float value)
    {
      self.top = new StyleLength(value);
    }

    public static void SetLeft(this IStyle self, float value)
    {
      self.left = new StyleLength(value);
    }

    public static float GetLeft(this VisualElement self)
    {
      return self.resolvedStyle.paddingLeft;
    }
    public static void SetLeft(this IStyle self, UIElements.Length length)
    {
      self.left = new StyleLength(length.value);
    }
    public static void SetMarginLeft(this IStyle self, UIElements.Length length)
    {
      self.marginLeft = new StyleLength(length.value);
    }
    public static void SetBottom(this IStyle self, float value)
    {
      self.bottom = new StyleLength(value);
    }

    public static void SetFontSize(this IStyle self, float value)
    {
      self.fontSize = new StyleLength((int)value);
    }

    public static float GetWidth(this IStyle style)
    {
	    return style.width.value.value;
    }

    public static void SetWidth(this IStyle self, float value)
    {
      self.width = new StyleLength(value);
    }

    public static void SetHeight(this IStyle self, float value, bool overrideMaxHeight = false)
    {
      self.height = new StyleLength(value);
      
      if (overrideMaxHeight)
	      self.maxHeight = new StyleLength(value);
    }


    public static float GetMaxHeight(this IStyle self)
    {
      return self.maxHeight.value.value;
    }

    public static void SetFlexDirection(this IStyle self, FlexDirection direction)
    {
	    self.flexDirection = direction;
    }

    public static void SetFlexGrow(this IStyle self, float value)
    {
	    self.flexGrow = new StyleFloat(value);
    }

    public static void SetImage(this Image self, Texture texture)
    {
      self.image = texture;
    }

    public static void BeamableFocus(this TextField self)
    {
      self.Q("unity-text-input").Focus();
    }

    public static void SetFontStyle(this Label self, FontStyle style)
    {
      self.style.unityFontStyleAndWeight = style;
    }

    public static void BeamableAppendAction(this DropdownMenu self, string title, Action<Vector2> callback, bool enabled
 = true)
    {
      self.AppendAction(title, evt => callback(evt.eventInfo.mousePosition), enabled ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
    }
  }
}
#endif


public static class UIElementsPolyfill2021
{
	public static void SetItemHeight(this ListView listView, float newHeight)
	{
#if UNITY_2021_2_OR_NEWER
		listView.fixedItemHeight = newHeight;
#else
		listView.itemHeight = (int)newHeight;
#endif
	}

	public static float GetItemHeight(this ListView listView)
	{
#if UNITY_2021_2_OR_NEWER
		return listView.fixedItemHeight;
#else
		return (float)listView.itemHeight;
#endif
	}


	public static void RefreshPolyfill(this ListView listView)
	{
#if UNITY_2021_2_OR_NEWER
		listView.RefreshItems();
#else
		listView.Refresh();
#endif
	}
}


#if UNITY_2020_1_OR_NEWER
public static class UIElementsPolyfill2020
{
  public static void BeamableOnSelectionsChanged(this ListView listView, Action<IEnumerable<object>> cb)
  {
    listView.onSelectionChange += cb;
  }
  public static void BeamableOnItemChosen(this ListView listView, Action<object> cb)
  {
    listView.onItemsChosen += set => cb(set.FirstOrDefault());
  }
}
#else
public static class UIElementsPolyfillPre2020
{
	public static void BeamableOnSelectionsChanged(this ListView listView, Action<List<object>> cb)
	{
		listView.onSelectionChanged += cb;
	}

	public static void BeamableOnItemChosen(this ListView listView, Action<object> cb)
	{
		listView.onItemChosen += cb;
	}
}
#endif



public static class UssLoader
{
	public static List<string> GetAvailableSheetPaths(string ussPath)
	{
		var ussPaths = new List<string> { ussPath };

		var darkPath = ussPath.Replace(".uss", ".dark.uss");
		var lightPath = ussPath.Replace(".uss", ".light.uss");
		var u2018Path = ussPath.Replace(".uss", ".2018.uss");
		var u2019Path = ussPath.Replace(".uss", ".2019.uss");
		var u2020Path = ussPath.Replace(".uss", ".2020.uss");
		var u2021Path = ussPath.Replace(".uss", ".2021.uss");
		var darkAvailable = File.Exists(darkPath);
		var lightAvailable = File.Exists(lightPath);
		var u2018Available = File.Exists(u2018Path);
		var u2019Available = File.Exists(u2019Path);
		var u2020Available = File.Exists(u2020Path);
		var u2021Available = File.Exists(u2021Path);

		if (EditorGUIUtility.isProSkin && darkAvailable)
		{
			ussPaths.Add(darkPath);
		}
		else if (!EditorGUIUtility.isProSkin && lightAvailable)
		{
			ussPaths.Add(lightPath);
		}

		if (u2018Available)
		{
#if UNITY_2018
			ussPaths.Add(u2018Path);
#endif
		}

		if (u2019Available)
		{
#if UNITY_2019
        ussPaths.Add(u2019Path);
#endif
		}

		if (u2020Available)
		{
#if UNITY_2020_1_OR_NEWER
			ussPaths.Add(u2020Path);
#endif
		}

		if (u2021Available)
		{
#if UNITY_2021_1_OR_NEWER // 2021 is the max supported version, so we forward lean and assume all uss works in 2022.
			ussPaths.Add(u2021Path);
#endif
		}

		return ussPaths;
	}
}
