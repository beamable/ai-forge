using System.Collections.Generic;
using System.Linq;

namespace Beamable.Common
{
	/// <summary>
	/// This type defines the passthrough for a %Beamable %Default %Query.
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	public class DefaultQuery
	{
		public string IdContainsConstraint;


		protected static bool SerializeIdRule(DefaultQuery query, out string str)
		{
			str = "";
			if (query.IdContainsConstraint == null)
			{
				return false;
			}
			str = $"{query.IdContainsConstraint}";
			return true;
		}


		public bool AcceptIdContains(string id)
		{
			if (IdContainsConstraint == null) return true;
			if (id == null) return false;
			return id.ToLower().Split('.').Last().Contains(IdContainsConstraint.ToLower());
		}

	}


	/// <summary>
	/// This type defines the passthrough for a %Beamable %Default %Query %Parser.
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	public static class DefaultQueryParser
	{
		public delegate void ApplyParseRule<in T>(string raw, T query) where T : DefaultQuery;

		public delegate bool SerializeRule<in T>(T query, out string serializedpart) where T : DefaultQuery;

		public static string ToString<T>(string existing, T query, List<SerializeRule<T>> serializeRules, Dictionary<string, ApplyParseRule<T>> parseRules)
			where T : DefaultQuery, new()
		{
			if (existing == null)
			{
				existing = "";
			}
			else
			{
				var standardParse = Parse<T>(existing, parseRules);

				if (standardParse.Equals(query))
				{
					return existing; // easy way out.
				}
			}

			var additionalParts = new List<string>();
			var partMap = new Dictionary<string, string>();

			foreach (var rule in serializeRules)
			{
				if (!rule.Invoke(query, out var clause)) continue;

				if (clause.Contains(":"))
				{
					var index = clause.IndexOf(':');
					var leftPart = clause.Substring(0, index);
					var rightPart = clause.Substring(index + 1);
					partMap[leftPart] = rightPart;
				}
				else if (!string.IsNullOrEmpty(clause))
				{
					partMap["id"] = clause;
				}
				if (!existing.Contains(clause))
				{
					additionalParts.Add(clause);
				}
			}

			var strParts = new List<string>();
			void HandleCouple(string leftPart, string rightPart)
			{

				if (string.IsNullOrEmpty(leftPart))
				{
					leftPart = "id";
				}

				var leftText = (leftPart.Equals("id") ? "" : $"{leftPart}:");

				if (partMap.TryGetValue(leftPart.Trim(), out var existingRightPart))
				{
					partMap.Remove(leftPart.Trim());

					strParts.Add($"{leftText}{existingRightPart}");
				}
				else if (!string.IsNullOrEmpty(leftPart))
				{
					strParts.Add($"{leftText}{rightPart}");
				}
				else
				{
					strParts.Add(rightPart);
				}
			}

			var buffer = "";
			var left = "";
			var right = "";

			for (var i = 0; i < existing.Length; i++)
			{
				var c = existing[i];
				switch (c)
				{
					case ':':
						left = buffer;
						buffer = "";
						break;
					case ',':
						// parse the buffer for a couple grouping.
						right = buffer;
						buffer = "";
						HandleCouple(left, right);
						left = "";
						right = "";
						break;
					default:
						buffer += c;
						break;
				}
			}

			right = buffer;
			if (!string.IsNullOrEmpty(left) || !string.IsNullOrEmpty(right))
			{
				HandleCouple(left, right);
			}


			var extraPartStr = "";
			if (partMap.Count > 0)
			{
				var partStr = partMap.Select(kvp => kvp.Key.Equals("id")
					? kvp.Value
					: $"{kvp.Key}:{kvp.Value}").ToList();

				extraPartStr = string.Join(", ", partStr);
				//strParts.AddRange(partStr);
			}
			var strOut = string.Join(",", strParts);
			if (strOut.EndsWith(",") || strOut.Length == 0)
			{
				strOut += extraPartStr;
			}
			else if (extraPartStr.Length > 0)
			{
				strOut += $", {extraPartStr}";
			}
			return strOut;
		}


		public static T Parse<T>(string raw, Dictionary<string, ApplyParseRule<T>> rules)
			where T : DefaultQuery, new()
		{
			var output = new T();
			if (string.IsNullOrEmpty(raw))
				return output;

			void ParseCouple(string leftPart, string rightPart) // tag: hello foo; id:tuna frank:man
			{
				leftPart = leftPart.Trim();
				rightPart = rightPart.Trim();

				if (string.IsNullOrEmpty(leftPart))
				{
					ApplyIdParse(rightPart, output);
				}
				else if (rules.TryGetValue(leftPart, out var rule))
				{
					rule?.Invoke(rightPart, output);
				}
				else
				{ // ???
				}
			}

			var buffer = "";
			var left = "";
			var right = "";

			for (var i = 0; i < raw.Length; i++)
			{
				var c = raw[i];
				switch (c)
				{
					case ':':
						left = buffer;
						buffer = "";
						break;
					case ',':
						// parse the buffer for a couple grouping.
						right = buffer;
						buffer = "";
						ParseCouple(left, right);
						left = "";
						right = "";
						break;
					default:
						buffer += c;
						break;
				}
			}

			right = buffer;
			if (!string.IsNullOrEmpty(left) || !string.IsNullOrEmpty(right))
			{
				ParseCouple(left, right);
			}
			return output;
		}

		public static void ApplyIdParse(string raw, DefaultQuery query)
		{
			query.IdContainsConstraint = raw;
		}
	}
}
