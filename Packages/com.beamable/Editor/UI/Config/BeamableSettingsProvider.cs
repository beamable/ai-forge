using System;
using System.Collections.Generic;
using System.Linq;
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
using static Beamable.Common.Constants.Features.Config;

namespace Beamable.Editor.Config
{
	public static class BeamableSettingsProvider
	{
		[MenuItem(
			MenuItems.Windows.Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE + "/" +
			Commons.OPEN + " " +
			MenuItems.Windows.Names.CONFIG_MANAGER,
			priority = MenuItems.Windows.Orders.MENU_ITEM_PATH_WINDOW_PRIORITY_2
		)]
		public static void Open()
		{
			ConfigManager.Initialize(forceCreation: true);
			SettingsService.OpenProjectSettings("Project/Beamable");
		}

		[SettingsProvider]
		public static SettingsProvider CreateBeamableProjectSettings()
		{
			try
			{
				var provider = new SettingsProvider($"Project/Beamable", SettingsScope.Project)
				{
					activateHandler = (searchContext, rootElement) =>
					{
						try
						{
							ConfigManager.Initialize(); // re-initialize every time the window is activated, so that we make sure the SO's always exist.

							if (ConfigManager.MissingAnyConfiguration)
							{
								var createButton = new Button(() =>
								{
									Open();
									SettingsService.NotifySettingsProviderChanged();
								})
								{ text = "Create Beamable Config Files" };
								var missingConfigs =
									string.Join(",\n", ConfigManager.MissingConfigurations.Select(d => $" - {d.Name}"));
								var lbl = new Label() { text = $"Welcome to Beamable! These configurations need to be created:\n{missingConfigs}" };
								lbl.AddTextWrapStyle();
								rootElement.Add(lbl);
								rootElement.Add(createButton);
							}

							var options = ConfigManager.GenerateOptions();

							var scroller = new ScrollView();
							rootElement.AddStyleSheet($"{BASE_UI_PATH}/ConfigWindow.uss");
							rootElement.Add(scroller);

							ConfigWindow.CreateFields(scroller, null, options, true);
						}
						catch (Exception)
						{
							// try to reset the assets.
							AssetDatabase.Refresh();
						}
					},
					keywords = new HashSet<string>(new[] { "Beamable" })
				};

				return provider;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				return null;
			}
		}

		public static SettingsProvider[] provider;

		[SettingsProviderGroup]
		public static SettingsProvider[] CreateBeamableProjectModuleSettings()
		{
			DelayCall(false);

			void DelayCall(bool notifyIfFound)
			{
				if (!BeamEditor.IsInitialized)
				{
					EditorApplication.delayCall += () => DelayCall(true);
					return;
				}

				try
				{
					ConfigManager.Initialize(); // re-initialize every time the window is activated, so that we make sure the SO's always exist.

					List<SettingsProvider> providers = new List<SettingsProvider>();

					foreach (BaseModuleConfigurationObject config in ConfigManager.ConfigObjects)
					{
						var options = ConfigManager.GenerateOptions(config);

						if (options.Count == 0)
						{
							continue;
						}

						var settingsProvider = new SettingsProvider($"Project/Beamable/{options[0].Module}", SettingsScope.Project)
						{
							activateHandler = (searchContext, rootElement) =>
							{
								options = ConfigManager.GenerateOptions(config);
								var scroller = new ScrollView();
								rootElement.AddStyleSheet($"{BASE_UI_PATH}/ConfigWindow.uss");
								rootElement.Add(scroller);
								ConfigWindow.CreateFields(scroller, null, options, false);
							},
							keywords = new HashSet<string>(options.Select(o => o.Name))
						};

						providers.Add(settingsProvider);
					}

					provider = providers.ToArray();
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}

				if (notifyIfFound)
					SettingsService.NotifySettingsProviderChanged();
			}

			return provider;
		}
	}
}
