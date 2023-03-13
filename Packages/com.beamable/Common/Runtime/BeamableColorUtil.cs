using System.Globalization;
using UnityEngine;

namespace Beamable.UI
{
	public static class BeamableColorUtil
	{
		const int REDSHIFT = 255 << 16;
		const int GREENSHIFT = 255 << 8;
		const int BLUESHIFT = 255;
		public static Color FromHex(string hex)
		{
			hex = hex[0] == '#' ? hex.Substring(1) : hex;
			var rgb = int.Parse(hex, NumberStyles.HexNumber);

			int red = rgb & REDSHIFT;
			int green = rgb & GREENSHIFT;
			int blue = rgb & BLUESHIFT;

			return new Color(red / (float)REDSHIFT, green / (float)GREENSHIFT, blue / (float)BLUESHIFT);
		}
	}
}
