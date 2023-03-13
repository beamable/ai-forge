using UnityEditor;

namespace Beamable.Editor.Config.Model
{
	public class ConfigOption
	{
		public SerializedProperty Property;
		public SerializedObject Object;
		public string Name;
		public string Help; // TODO: Make this work. The tooltip property is broken
		public string Module;

		public ConfigOption()
		{

		}

		public ConfigOption(SerializedObject obj, BaseModuleConfigurationObject config, SerializedProperty property, string help)
		{
			Object = obj;
			Property = property;
			Name = property.displayName;
			Help = help;
			Module = config.GetType().Name.Replace("Configuration", "");
		}
	}

}
