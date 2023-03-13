using System;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beamable.UI.Buss
{
	public abstract class BaseAssetProperty : DefaultBussProperty, IBussProperty
	{
		public int AssetSerializationKey = -1;

		public abstract Object GenericAsset
		{
			get;
			set;
		}

		public abstract Type GetAssetType();
		public abstract IBussProperty CopyProperty();
	}

	public abstract class BaseAssetProperty<T> : BaseAssetProperty where T : Object
	{
		protected T _asset;

		public T Asset
		{
			get => _asset;
			set => _asset = value;
		}

		public override Object GenericAsset
		{
			get => Asset;
			set => Asset = value as T;
		}

		public override Type GetAssetType() => typeof(T);
	}

	[Serializable]
	public class SpriteBussProperty : BaseAssetProperty<Sprite>, ISpriteBussProperty
	{
		public Sprite SpriteValue => Asset;

		public SpriteBussProperty() { }

		public SpriteBussProperty(Sprite sprite)
		{
			Asset = sprite;
		}

		public override IBussProperty CopyProperty()
		{
			return new SpriteBussProperty(Asset);
		}
	}

	[Serializable]
	public class FontBussAssetProperty : BaseAssetProperty<TMP_FontAsset>, IFontBussProperty
	{
		public TMP_FontAsset FontAsset => Asset;

		public FontBussAssetProperty() { }

		public FontBussAssetProperty(TMP_FontAsset asset)
		{
			Asset = asset;
		}

		public override IBussProperty CopyProperty()
		{
			return new FontBussAssetProperty(Asset);
		}
	}
}
