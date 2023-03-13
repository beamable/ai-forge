using System.Collections.Generic;
using UnityEngine;
using static Beamable.Common.Constants.MenuItems.Assets.Orders;

namespace Beamable.Editor.Assistant
{
	/// <summary>
	/// Holds a mapping of UXML/USS files to ids used by <see cref="BeamHintDetailConverterProvider"/> and <see cref="BeamHintHeaderVisualElement"/> to render out
	/// hint details. 
	/// </summary>
	[CreateAssetMenu(fileName = "BeamHintDetailsConfig", menuName = "Beamable/Assistant/Hint Details", order = MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_2)]
	public class BeamHintDetailsConfig : ScriptableObject
	{
		[Tooltip("The id you want to reference in your " + nameof(BeamHintDetailConverterAttribute) + "s in order to map these UXML/USS files to specific set of hint(s).")]
		public string Id;

		[Tooltip("The path to a UXML file that'll be added to the BeamHintHeaderVisualElement element when rendering details of the hint.")]
		public string UxmlFile;
		[Tooltip("The paths to USS files that'll be added to the BeamHintHeaderVisualElement element when rendering details of the hint.")]
		public List<string> StylesheetsToAdd;

		private void OnValidate()
		{
			if (string.IsNullOrEmpty(Id))
				Id = name;
		}
	}
}
