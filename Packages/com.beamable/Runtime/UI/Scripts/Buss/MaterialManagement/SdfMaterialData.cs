using System;

namespace Beamable.UI.Sdf
{
	public struct SdfMaterialData
	{
		public int baseMaterialID;
		//public int mainTextureID;
		public int secondaryTextureID;
		public SdfImage.SdfMode imageMode;
		public SdfShadowMode shadowMode;
		public SdfBackgroundMode backgroundMode;
		public bool isBackgroundTexMain;

		public override bool Equals(object other)
		{
			return other is SdfMaterialData d
				   && d.baseMaterialID == baseMaterialID
				   //&& d.mainTextureID == mainTextureID
				   && d.secondaryTextureID == secondaryTextureID
				   && d.imageMode == imageMode
				   && d.shadowMode == shadowMode
				   && d.backgroundMode == backgroundMode
				   && d.isBackgroundTexMain == isBackgroundTexMain;
		}

		public override int GetHashCode()
		{
			unchecked // Allow arithmetic overflow, numbers will just "wrap around"
			{
				int hashcode = baseMaterialID.GetHashCode();
				hashcode = (((hashcode << 5) + hashcode) ^ secondaryTextureID.GetHashCode());
				hashcode = (((hashcode << 5) + hashcode) ^ imageMode.GetHashCode());
				hashcode = (((hashcode << 5) + hashcode) ^ shadowMode.GetHashCode());
				hashcode = (((hashcode << 5) + hashcode) ^ backgroundMode.GetHashCode());
				hashcode = (((hashcode << 5) + hashcode) ^ isBackgroundTexMain.GetHashCode());
				return hashcode;
			}
		}
	}

	public enum SdfShadowMode
	{
		Default,
		Inner
	}

	public enum SdfBackgroundMode
	{
		Default,
		Outline,
		Full
	}
}
