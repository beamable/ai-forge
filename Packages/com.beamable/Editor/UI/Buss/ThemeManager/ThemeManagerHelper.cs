using System;

namespace Beamable.Editor.UI.Buss
{
	public static class ThemeManagerHelper
	{
		public static string FormatKey(string input)
		{
			for (int i = input.Length - 1; i >= 0; i--)
			{
				char currentChar = input[i];

				if (i == 0)
				{
					input = Char.ToUpperInvariant(currentChar) + input.Substring(1);
				}

				if (char.IsUpper(currentChar))
				{
					input = input.Insert(i, " ");
				}
			}

			return input;
		}
	}
}
