/// FROM https://bitbucket.org/UnityUIExtensions/unity-ui-extensions/src/master/
/// Credit SimonDarksideJ
/// Required for scrollbar support to work across ALL scroll snaps


namespace Beamable.UnityEngineClone.UI.Extensions
{
	internal interface IScrollSnap
	{
		void ChangePage(int page);
		void SetLerp(bool value);
		int CurrentPage();
		void StartScreenChange();
	}
}
