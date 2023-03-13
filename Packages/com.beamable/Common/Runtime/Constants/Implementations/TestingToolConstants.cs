namespace Beamable.Common
{
	public static partial class Constants
	{
		public static partial class Features
		{
			public static partial class TestingTool
			{
				public static class FileNames
				{
					public const string TEST_CREATOR = "TestScenariosCreator";
					public const string TEST_CONFIG = "TestingToolConfig";
					public const string TEST_SCENARIOS_RUNTIME = "TestScenariosRuntime";
					public const string MAIN_TEST_SCENE = "MainMenu";
					public const string NEW_SCENE = "NewTestScene_RenameMe";
					public const string TEST_SCENE_TEMPLATE = "TestSceneTemplate";
				}

				public static class Directories
				{
					public const string BASE_PATH = "Assets/TestingTool";
					public const string TEST_TOOL_RESOURCES_DIRECTORY = BASE_PATH + "/Resources";
					public const string TEST_TOOL_SCENES_PATH = BASE_PATH + "/Scenes";
					public const string TEST_TOOL_SCENES_TEMPLATE_PATH = TEST_TOOL_SCENES_PATH + "/SceneTemplate";

					public const string TEST_SCENARIOS_CREATOR_ASSET_PATH = BASE_PATH + "/" + FileNames.TEST_CREATOR + ".asset";
					public const string TEST_SCENARIOS_RUNTIME_ASSET_PATH = TEST_TOOL_RESOURCES_DIRECTORY + "/" + FileNames.TEST_SCENARIOS_RUNTIME + ".asset";
					public const string TEMPLATE_SCENE_PATH = TEST_TOOL_SCENES_TEMPLATE_PATH + "/" + FileNames.TEST_SCENE_TEMPLATE + ".unity";
					public const string MAIN_MENU_SCENE_PATH = BASE_PATH + "/" + FileNames.MAIN_TEST_SCENE + ".unity";
					public const string CONFIG_ASSET_PATH = BASE_PATH + "/" + FileNames.TEST_CONFIG + ".asset";

					public static string TEST_SCENE_DATA_PATH(string sceneName) => $"{TEST_TOOL_SCENES_PATH}/{sceneName}.unity";
				}

			}
		}
	}
}
