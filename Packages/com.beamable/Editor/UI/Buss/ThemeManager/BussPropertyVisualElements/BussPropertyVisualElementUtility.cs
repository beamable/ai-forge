using Beamable.Editor.UI.Components;

namespace Beamable.UI.Buss
{
	public static class BussPropertyVisualElementUtility
	{
		public static BussPropertyVisualElement GetVisualElement(this BussPropertyProvider propertyProvider)
		{
			var property = propertyProvider.GetProperty();
			return GetVisualElement(property);
		}

		public static BussPropertyVisualElement GetVisualElement(this IBussProperty property)
		{
			switch (property)
			{
				case FloatBussProperty floatProperty:
					return new FloatBussPropertyVisualElement(floatProperty);
				case Vector2BussProperty vector2BussProperty:
					return new Vector2BussPropertyVisualElement(vector2BussProperty);
				case SingleColorBussProperty colorProperty:
					return new ColorBussPropertyVisualElement(colorProperty);
				case VertexColorBussProperty vertexColorProperty:
					return new VertexColorBussPropertyVisualElement(vertexColorProperty);
				case TextAlignmentOptionsBussProperty textAlignmentProperty:
					return new TextAlignmentBussPropertyVisualElement(textAlignmentProperty);
				case EnumBussProperty enumBussProperty:
					return new EnumBussPropertyVisualElement(enumBussProperty);
				case BaseAssetProperty assetProperty:
					return new AssetBussPropertyVisualElement(assetProperty);
				default:
					return new NotImplementedBussPropertyVisualElement(property);
			}
		}
	}
}
