using Beamable.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Beamable.UI.Buss
{
	public static class BussStyleSheetUtility
	{
		public static bool IsValidVariableName(string name) => name?.StartsWith("--") ?? false;

		public static bool TryAddProperty(this BussStyleDescription target, string key, IBussProperty property)
		{
			var isKeyValid = BussStyle.IsKeyValid(key) || IsValidVariableName(key);
			if (isKeyValid && !target.HasProperty(key) && BussStyle.GetBaseType(key).IsInstanceOfType(property))
			{
				var propertyProvider = BussPropertyProvider.Create(key, property.CopyProperty());
				target.Properties.Add(propertyProvider);
				return true;
			}

			return false;
		}

		public static bool HasProperty(this BussStyleDescription target, string key)
		{
			return target.Properties.Any(p => p.Key == key);
		}

		public static BussPropertyProvider GetPropertyProvider(this BussStyleDescription target, string key)
		{
			return target.Properties.FirstOrDefault(p => p.Key == key);
		}

		public static IBussProperty GetProperty(this BussStyleDescription target, string key)
		{
			return target.GetPropertyProvider(key)?.GetProperty();
		}

		public static IEnumerable<BussPropertyProvider> GetVariablePropertyProviders(this BussStyleDescription target)
		{
			return target.Properties.Where(p => IsValidVariableName(p.Key));
		}

		public static void AssignAssetReferencesFromReferenceList(this BussStyleDescription style,
																  List<Object> assetReferences)
		{
			foreach (BussPropertyProvider propertyProvider in style.Properties)
			{
				var property = propertyProvider.GetProperty();
				if (property is BaseAssetProperty assetProperty)
				{
					if (assetProperty.AssetSerializationKey >= 0 &&
						assetProperty.AssetSerializationKey < assetReferences.Count)
					{
						assetProperty.GenericAsset = assetReferences[assetProperty.AssetSerializationKey];
					}
					else
					{
						assetProperty.GenericAsset = null;
						assetProperty.AssetSerializationKey = -1;
					}
				}
			}
		}

		public static void PutAssetReferencesInReferenceList(this BussStyleDescription style,
															 List<Object> assetReferences)
		{
			if (style == null || style.Properties == null)
			{
				return;
			}

			foreach (BussPropertyProvider propertyProvider in style.Properties)
			{
				var property = propertyProvider.GetProperty();
				if (property is BaseAssetProperty assetProperty)
				{
					var asset = assetProperty.GenericAsset;
					if (asset != null)
					{
						var index = assetReferences.IndexOf(asset);
						if (index == -1)
						{
							index = assetReferences.Count;
							assetReferences.Add(asset);
						}

						assetProperty.AssetSerializationKey = index;
					}
					else
					{
						assetProperty.AssetSerializationKey = -1;
					}
				}
			}
		}

		public static void CopySingleStyle(BussStyleSheet targetStyleSheet, BussStyleRule style)
		{
			if (style == null)
			{
				BeamableLogger.LogWarning("Style to copy can't be null");
				return;
			}
			BeamableUndoUtility.Undo(targetStyleSheet, "Copy Style");

			BussStyleRule rule = BussStyleRule.Create(style.SelectorString, new List<BussPropertyProvider>());

			foreach (BussPropertyProvider propertyProvider in style.Properties)
			{
				rule.TryAddProperty(propertyProvider.Key, propertyProvider.GetProperty().CopyProperty());
			}

			targetStyleSheet.Styles.Add(rule);
#if UNITY_EDITOR
			EditorUtility.SetDirty(targetStyleSheet);
			AssetDatabase.SaveAssets();
#endif
			targetStyleSheet.TriggerChange();
		}

		public static void RemoveSingleStyle(BussStyleSheet targetStyleSheet, BussStyleRule style)
		{
			targetStyleSheet.RemoveStyle(style);
#if UNITY_EDITOR
			EditorUtility.SetDirty(targetStyleSheet);
			AssetDatabase.SaveAssets();
#endif
			targetStyleSheet.TriggerChange();
		}

		public static void CreateNewStyleSheetWithInitialRules(string fileName, List<BussStyleRule> styles)
		{
			BussStyleSheet newStyleSheet = ScriptableObject.CreateInstance<BussStyleSheet>();

			foreach (BussStyleRule styleRule in styles)
			{
				CopySingleStyle(newStyleSheet, styleRule);
			}

#if UNITY_EDITOR
			AssetDatabase.CreateAsset(newStyleSheet, $"Assets/Resources/{fileName}.asset");
			AssetDatabase.SaveAssets();
#endif
		}
	}
}
