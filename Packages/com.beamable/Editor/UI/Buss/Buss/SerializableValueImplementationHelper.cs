using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Editor.UI.Buss
{
	public static class SerializableValueImplementationHelper
	{

		private static Dictionary<Type, ImplementationData> _data = new Dictionary<Type, ImplementationData>();
		private static Dictionary<Tuple<Type, string>, ImplementationData> _specialRulesData = new Dictionary<Tuple<Type, string>, ImplementationData>();

		public static ImplementationData Get(Type baseType)
		{
			if (!_data.TryGetValue(baseType, out var data))
			{
				data = new ImplementationData(baseType);
				_data[baseType] = data;
			}
			return data;
		}

		public static ImplementationData GetWithSpecialRule(string rule, Type baseType, params Type[] additionalTypes)
		{
			if (!_specialRulesData.TryGetValue(new Tuple<Type, string>(baseType, rule), out var data))
			{
				data = new ImplementationData(baseType, additionalTypes);
				_specialRulesData[new Tuple<Type, string>(baseType, rule)] = data;
			}
			return data;
		}

		public class ImplementationData
		{
			public readonly Type baseType;
			public readonly Type[] subTypes;
			public readonly GUIContent[] labels;

			internal ImplementationData(Type baseType, Type[] additionalTypes = null)
			{
				this.baseType = baseType;
				var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t =>
					t.IsSerializable && t.IsClass && baseType.IsAssignableFrom(t)).ToList();
				if (additionalTypes != null)
				{
					types.AddRange(additionalTypes);
				}
				types.Insert(0, null);
				subTypes = types.ToArray();
				labels = subTypes.Select(t => new GUIContent(t == null ? "Null" : t.Name)).ToArray();
			}
		}
	}
}
