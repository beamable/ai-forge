using Beamable.UI.Sdf;
using Beamable.UI.Tweening;
using System;
using TMPro;

namespace Beamable.UI.Buss
{
	public abstract class EnumBussProperty : DefaultBussProperty, IEnumBussProperty
	{
		public abstract Enum EnumValue
		{
			get;
			set;
		}

		public abstract IBussProperty CopyProperty();

		public T CastEnumValue<T>() where T : Enum
		{
			return (T)EnumValue;
		}
	}

	public abstract class EnumBussProperty<T> : EnumBussProperty where T : Enum
	{
		public T Enum;

		public override Enum EnumValue
		{
			get => Enum;
			set => Enum = (T)value;
		}

		protected EnumBussProperty() { }

		protected EnumBussProperty(T @enum)
		{
			Enum = @enum;
		}
	}

	[Serializable]
	public class ImageTypeBussProperty : EnumBussProperty<SdfImage.ImageType>
	{
		public ImageTypeBussProperty() { }
		public ImageTypeBussProperty(SdfImage.ImageType @enum) : base(@enum) { }

		public override IBussProperty CopyProperty()
		{
			return new ImageTypeBussProperty(Enum);
		}
	}

	// TODO: can't we move BorderMode class into Beamable.UI.BUSS namespace?
	[Serializable]
	public class SdfModeBussProperty : EnumBussProperty<SdfImage.SdfMode>
	{
		public SdfModeBussProperty() { }
		public SdfModeBussProperty(SdfImage.SdfMode @enum) : base(@enum) { }

		public override IBussProperty CopyProperty()
		{
			return new SdfModeBussProperty(Enum);
		}
	}

	[Serializable]
	public class BorderModeBussProperty : EnumBussProperty<SdfImage.BorderMode>
	{
		public BorderModeBussProperty() { }
		public BorderModeBussProperty(SdfImage.BorderMode @enum) : base(@enum) { }

		public override IBussProperty CopyProperty()
		{
			return new BorderModeBussProperty(Enum);
		}
	}

	[Serializable]
	public class BackgroundModeBussProperty : EnumBussProperty<SdfBackgroundMode>
	{
		public BackgroundModeBussProperty() { }
		public BackgroundModeBussProperty(SdfBackgroundMode @enum) : base(@enum) { }

		public override IBussProperty CopyProperty()
		{
			return new BackgroundModeBussProperty(Enum);
		}
	}

	[Serializable]
	public class ShadowModeBussProperty : EnumBussProperty<SdfShadowMode>
	{
		public ShadowModeBussProperty() { }
		public ShadowModeBussProperty(SdfShadowMode @enum) : base(@enum) { }

		public override IBussProperty CopyProperty()
		{
			return new ShadowModeBussProperty(Enum);
		}
	}

	[Serializable]
	public class EasingBussProperty : EnumBussProperty<Easing>
	{
		public EasingBussProperty() { }

		public EasingBussProperty(Easing easing) : base(easing) { }

		public override IBussProperty CopyProperty()
		{
			return new EasingBussProperty(Enum);
		}
	}

	[Serializable]
	public class TextAlignmentOptionsBussProperty : EnumBussProperty<TextAlignmentOptions>
	{
		public TextAlignmentOptionsBussProperty() { }

		public TextAlignmentOptionsBussProperty(TextAlignmentOptions textAlignmentOptions) :
			base(textAlignmentOptions)
		{ }

		public override IBussProperty CopyProperty()
		{
			return new TextAlignmentOptionsBussProperty(Enum);
		}
	}

	[Serializable]
	public class MainTextureBussProperty : EnumBussProperty<MainTextureBussProperty.Options>
	{
		public MainTextureBussProperty() { }

		public MainTextureBussProperty(Options option) :
			base(option)
		{ }

		public override IBussProperty CopyProperty()
		{
			return new MainTextureBussProperty(Enum);
		}

		public enum Options
		{
			SdfSprite,
			BackgroundSprite
		}
	}

	[Serializable]
	public class NineSliceSourceBussProperty : EnumBussProperty<SdfImage.NineSliceSource>
	{
		public NineSliceSourceBussProperty() { }

		public NineSliceSourceBussProperty(SdfImage.NineSliceSource option) :
			base(option)
		{ }

		public override IBussProperty CopyProperty()
		{
			return new NineSliceSourceBussProperty(Enum);
		}
	}
}
