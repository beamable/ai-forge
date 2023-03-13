using Beamable.Content;
using Beamable.Editor.Content;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Beamable.Editor
{
	public class BuildPreProcessor : IPreprocessBuildWithReport
	{
		public int callbackOrder { get; }

#if !UNITY_STANDALONE
		public void OnPreprocessBuild(BuildReport report)
		{
			if (CoreConfiguration.Instance.PreventCodeStripping)
			{
				BeamableLinker.GenerateLinkFile();
			}

			if (CoreConfiguration.Instance.PreventAddressableCodeStripping)
			{
				BeamableLinker.GenerateAddressablesLinkFile();
			}
		}
#else
        public async void OnPreprocessBuild(BuildReport report)
        {
            if (ContentConfiguration.Instance.BakeContentOnBuild)
            {
                await ContentIO.BakeContent();
            }
			if (CoreConfiguration.Instance.PreventCodeStripping)
            {
				BeamableLinker.GenerateLinkFile();
            }
        }
#endif
	}
}
