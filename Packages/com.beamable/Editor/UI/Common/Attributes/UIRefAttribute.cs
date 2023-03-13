using Beamable.Common;
using Beamable.Editor.UI.Common;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace Beamable.Editor.UI.Components
{
	[AttributeUsage(AttributeTargets.Field)]
	public class UIRefAttribute : Attribute
	{
		private static readonly Regex idRegex = new Regex("[a-zA-Z0-9]*");
		private readonly string _id;
		private readonly string[] _classes;

		public UIRefAttribute(string id = null, params string[] classes)
		{
			_id = id;
			_classes = classes;
		}

		private string GetId(string defaultId)
		{
			if (string.IsNullOrWhiteSpace(_id))
			{
				return idRegex.Match(defaultId).Value;
			}

			return _id;
		}

		public void AssignRef(BeamableBasicVisualElement element, FieldInfo field)
		{
			var id = GetId(field.Name);
			var found = element.Q(id, _classes);
			if (found == null)
			{
				BeamableLogger.LogWarning($"No visual element found for UIRef (ID: {id}).");
			}
			else
			{
				field.SetValue(element, found);
			}
		}
	}
}
