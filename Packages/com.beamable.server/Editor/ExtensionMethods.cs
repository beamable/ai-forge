using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Editor
{
	public static class ExtensionMethods
	{
		/// <summary>
		/// Tries to ellipse text if given length is lower than input text length
		/// </summary>
		/// <param name="inputText">Input text</param>
		/// <param name="length">Distance at which the text will be trimmed</param>
		/// <param name="outputText">Output text</param>
		/// <returns>The result of text operation. Returns true if text was trimmed</returns>
		public static bool TryEllipseText(this string inputText, int length, out string outputText)
		{
			outputText = inputText;
			if (length >= inputText.Length)
				return false;
			outputText = $"{inputText.Substring(0, length)}...";
			return true;
		}

		public static List<string> SplitStringIntoParts(this string str, int chunkSize)
		{
			if (string.IsNullOrWhiteSpace(str))
				return new List<string>();

			var fullDataChunksCount = Mathf.FloorToInt(str.Length / (float)chunkSize);
			var lastDataChunkSize = str.Length - (chunkSize * fullDataChunksCount);
			var result = Enumerable.Range(0, fullDataChunksCount)
								   .Select(i => str.Substring(i * chunkSize, chunkSize))
								   .ToList();

			result.Add(str.Substring(chunkSize * fullDataChunksCount, lastDataChunkSize));
			return !result.Any() ? new List<string> { str } : result;
		}
	}
}
