using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using static Beamable.Common.Constants.MenuItems.Windows;

namespace Beamable.Editor
{
	public static class BeamableLinker
	{
		private static string[] THIRD_PARTY_ASSEMBLIES = new[]
		{
			"PubNub",
			"VirtualList",
			"UnityUIExtensions"
		};

		[MenuItem(Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES + "/Generate Addressables Link File")]
		public static void GenerateAddressablesLinkFile()
		{
			var linkPath = "Assets/Beamable/Resources/AddressablesLinker/link.xml";
			var link = @"
<linker>
    <assembly fullname=""Unity.ResourceManager, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"" preserve=""all"">
        <type fullname=""UnityEngine.ResourceManagement.ResourceProviders.LegacyResourcesProvider"" preserve=""all"" />
        <type fullname=""UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider"" preserve=""all"" />
        <type fullname=""UnityEngine.ResourceManagement.ResourceProviders.BundledAssetProvider"" preserve=""all"" />
        <type fullname=""UnityEngine.ResourceManagement.ResourceProviders.InstanceProvider"" preserve=""all"" />
        <type fullname=""UnityEngine.ResourceManagement.AsyncOperations"" preserve=""all"" />
    </assembly>
    <assembly fullname=""Unity.Addressables, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"" preserve=""all"">
        <type fullname=""UnityEngine.AddressableAssets.Addressables"" preserve=""all"" />
    </assembly>
</linker>
";
			var sb = new StringBuilder();
			sb.Append(link);
			var xml = sb.ToString();
			var alreadyExists = (File.Exists(linkPath) && File.ReadAllText(linkPath) == xml);
			if (alreadyExists) return;
			Directory.CreateDirectory(Path.GetDirectoryName(linkPath));
			File.WriteAllText(linkPath, xml);
		}

		[MenuItem(Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES + "/Generate Link File")]
		public static void GenerateLinkFile()
		{
			var linkPath = "Assets/Beamable/Resources/link.xml";
			var assemblies = new HashSet<string>();

			foreach (var asm in THIRD_PARTY_ASSEMBLIES)
			{
				assemblies.Add(asm);
			}

			var otherAssemblies = CoreConfiguration.Instance.AssembliesToSweep.Where(a => a.Contains("Beamable") && !a.Contains("Editor"));
			foreach (var asm in otherAssemblies)
			{
				assemblies.Add(asm);
			}

			var sb = new StringBuilder();
			sb.AppendLine("<linker>");
			foreach (var asm in assemblies)
			{
				sb.AppendLine($"<assembly fullname=\"{asm}\" preserve=\"all\"/>");
			}
			sb.AppendLine("</linker>");
			var xml = sb.ToString();

			var alreadyExists = (File.Exists(linkPath) && File.ReadAllText(linkPath) == xml);
			if (alreadyExists) return;
			File.WriteAllText(linkPath, xml);
		}
	}
}
