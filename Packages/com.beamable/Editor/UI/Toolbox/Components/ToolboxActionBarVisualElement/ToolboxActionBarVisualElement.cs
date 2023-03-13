using Beamable.Editor.Config;
using Beamable.Editor.Content;
using Beamable.Editor.Environment;
using Beamable.Editor.Login.UI;
using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.Toolbox.UI.Components;
using Beamable.Editor.UI.Buss;
using Beamable.Editor.UI.Components;
using System;
using System.Collections.Generic;
#if UNITY_2017_1_OR_NEWER && !UNITY_2019_3_OR_NEWER
using System.Reflection;
#endif
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

using static Beamable.Common.Constants;

namespace Beamable.Editor.Toolbox.Components
{
	// TODO: TD213896
	public class ToolboxActionBarVisualElement : ToolboxComponent
	{
		public new class UxmlFactory : UxmlFactory<ToolboxActionBarVisualElement, UxmlTraits> { }

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlStringAttributeDescription customText = new UxmlStringAttributeDescription
			{
				name = "custom-text",
				defaultValue = "nada"
			};

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as ToolboxActionBarVisualElement;
			}
		}

		public ToolboxActionBarVisualElement() : base(nameof(ToolboxActionBarVisualElement)) { }

		private IToolboxViewService Model { get; set; }

		private Button _categoryButton;
		private Button _typeButton;
		private Button _infoButton;
		private Button _microservicesButton;
		private Button _accountButton;

		public event Action OnInfoButtonClicked;

		public override void Refresh()
		{
			base.Refresh();

			Model = Provider.GetService<IToolboxViewService>();

			var contentButton = Root.Q<Button>("contentManager");
			contentButton.clickable.clicked += async () => { await ContentManagerWindow.Init(); };
			contentButton.tooltip = Tooltips.Toolbox.CONTENT;

			var skinningButton = Root.Q<Button>("skinning");
			skinningButton.clickable.clicked += ThemeManager.Init;
			skinningButton.tooltip = Tooltips.Toolbox.THEME_MANAGER;

			var globalConfigButton = Root.Q<Button>("globalConfig");
			globalConfigButton.clickable.clicked += () =>
			{
				BeamableSettingsProvider.Open();
				//                ConfigWindow.Init();
			};
			globalConfigButton.tooltip = Tooltips.Toolbox.CONFIG;

			_microservicesButton = Root.Q<Button>("microservice");
			_microservicesButton.clickable.clicked += () =>
			{
				MicroservicesButton_OnClicked(_microservicesButton.worldBound);
			};
			_microservicesButton.tooltip = Tooltips.Toolbox.MICROSERVICE;

			var filterBox = Root.Q<SearchBarVisualElement>();
			filterBox.SetValueWithoutNotify(Model.FilterText);
			filterBox.OnSearchChanged += FilterBox_OnTextChanged;
			Model.OnQueryChanged += () => { filterBox.SetValueWithoutNotify(Model.FilterText); };

			_typeButton = Root.Q<Button>("typeButton");
			_typeButton.clickable.clicked += () => { TypeButton_OnClicked(_typeButton.worldBound); };
			_typeButton.tooltip = Tooltips.Toolbox.LAYOUT;

			_categoryButton = Root.Q<Button>("CategoryButton");
			_categoryButton.clickable.clicked += () => { CategoryButton_OnClicked(_categoryButton.worldBound); };
			_categoryButton.tooltip = Tooltips.Toolbox.TAG;

			_infoButton = Root.Q<Button>("infoButton");
			_infoButton.clickable.clicked += () => { OnInfoButtonClicked?.Invoke(); };
			_infoButton.tooltip = Tooltips.Toolbox.DOCUMENT;

			_accountButton = Root.Q<Button>("accountButton");
			_accountButton.clickable.clicked += () =>
			{
				var wnd = LoginWindow.Init();
				Rect popupWindowRect = BeamablePopupWindow.GetLowerRightOfBounds(_accountButton.worldBound);
				wnd.position = new Rect(popupWindowRect.x - wnd.minSize.x, popupWindowRect.y + 10, wnd.minSize.x,
										wnd.minSize.y);
			};
			_accountButton.tooltip = Tooltips.Toolbox.MY_ACCOUNT;
		}

		private void FilterBox_OnTextChanged(string filter)
		{
			Model.SetQuery(filter);
		}

		private void TypeButton_OnClicked(Rect visualElementBounds)
		{
			Rect popupWindowRect = BeamablePopupWindow.GetLowerLeftOfBounds(visualElementBounds);

			var content = new TypeDropdownVisualElement();
			content.Model = Model;
			var wnd = BeamablePopupWindow.ShowDropdown("Type", popupWindowRect, new Vector2(100, 60), content);

			content.Refresh();
		}

		private void CategoryButton_OnClicked(Rect visualElementBounds)
		{
			Rect popupWindowRect = BeamablePopupWindow.GetLowerLeftOfBounds(visualElementBounds);

			var content = new CategoryDropdownVisualElement();
			content.Model = Model;
			var wnd = BeamablePopupWindow.ShowDropdown("Tags", popupWindowRect, new Vector2(200, 250), content);
			content.Refresh();
		}

		private void MicroservicesButton_OnClicked(Rect visualElementBounds)
		{
			Rect popupWindowRect = BeamablePopupWindow.GetLowerLeftOfBounds(visualElementBounds);

			BeamablePackages.GetServerPackage().Then(meta =>
			{
				if (meta.IsPackageAvailable)
				{
					BeamablePackages.ShowServerWindow();
					return;
				}

				var content = new InstallServerVisualElement { Model = meta };
				var wnd = BeamablePopupWindow.ShowDropdown("Install Microservices", popupWindowRect,
														   new Vector2(250, 185), content);
				content.OnClose += () => wnd.Close();
				content.OnInfo += () =>
				{
					OnInfoButtonClicked?.Invoke();
					wnd.Close();
				};
				content.OnDone += () =>
				{
					EditorApplication.delayCall += BeamablePackages.ShowServerWindow;
					// recompile scripts after import to make MMV2 window refresh properly
#if UNITY_2019_3_OR_NEWER
					EditorApplication.delayCall += UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation;
#elif UNITY_2017_1_OR_NEWER
					void RecompileScripts()
					{
						var editorAssembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
						var editorCompilationInterfaceType = editorAssembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
						var dirtyAllScriptsMethod = editorCompilationInterfaceType.GetMethod("DirtyAllScripts", BindingFlags.Static | BindingFlags.Public);
						dirtyAllScriptsMethod.Invoke(editorCompilationInterfaceType, null);
					}

					EditorApplication.delayCall += RecompileScripts;
#endif
					wnd.Close();
				};
				content.Refresh();
			});
		}
	}
}
