
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace Beamable.Editor.UI.Common
{
	/// <summary>
	/// This class is needed only to make ClearSelection a public method instead of protected.
	/// </summary>
	public class ExtendedListView : ListView
	{
		public new void ClearSelection()
		{
			base.ClearSelection();
		}
	}

}
