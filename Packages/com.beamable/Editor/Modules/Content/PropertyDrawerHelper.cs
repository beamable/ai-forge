using Beamable.Common.Content;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Content
{
	public static class PropertyDrawerHelper
	{
		/// <summary>
		/// Respect the [Tooltip] attribute if exists, otherwise fallback
		/// </summary>
		/// <param name="fieldInfo"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public static string SetTooltipWithFallback(FieldInfo fieldInfo, SerializedProperty property)
		{
			string tooltipString = "";

			TooltipAttribute tooltipAttribute = null;
			if (fieldInfo != null)
			{
				tooltipAttribute = fieldInfo.GetCustomAttribute<TooltipAttribute>();

				if (tooltipAttribute != null)
				{
					tooltipString = tooltipAttribute.tooltip;
				}
			}

			if (string.IsNullOrEmpty(tooltipString))
			{
				tooltipString = property.tooltip;
			}

			if (string.IsNullOrEmpty(tooltipString))
			{
				tooltipString = ContentObject.TooltipNotFoundDebugFallback1a;
			}

			return tooltipString;
		}
	}
}
