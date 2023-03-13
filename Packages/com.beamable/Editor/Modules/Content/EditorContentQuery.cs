using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Editor.Content.Models;
using System.Collections.Generic;

namespace Beamable.Editor.Content
{
	public class EditorContentQuery : ContentQuery
	{
		public bool HasValidationConstraint, HasStatusConstraint;
		public ContentValidationStatus ValidationConstraint;
		public ContentModificationStatus StatusConstraint;


		protected static Dictionary<string, DefaultQueryParser.ApplyParseRule<EditorContentQuery>> EditorRules;
		protected static List<DefaultQueryParser.SerializeRule<EditorContentQuery>> EditorSerializeRules;
		static EditorContentQuery()
		{
			EditorRules = new Dictionary<string, DefaultQueryParser.ApplyParseRule<EditorContentQuery>>
		 {
			{"valid", ApplyValidRule},
			{"status", ApplyStatusRule}
		 };
			foreach (var kvp in StandardRules)
			{
				EditorRules.Add(kvp.Key, kvp.Value);
			}

			EditorSerializeRules = new List<DefaultQueryParser.SerializeRule<EditorContentQuery>>
		 {
			SerializeValidRule, SerializeStatusRule
		 };
			EditorSerializeRules.AddRange(StandardSerializeRules);
		}



		private static bool SerializeValidRule(EditorContentQuery query, out string str)
		{
			str = "";
			if (query.HasValidationConstraint)
			{
				str = $"valid:{(query.ValidationConstraint == ContentValidationStatus.VALID ? "y" : "n")}";
				return true;
			}
			return false;
		}

		private static bool SerializeStatusRule(EditorContentQuery query, out string str)
		{
			str = "";
			if (query.HasStatusConstraint)
			{
				str = $"status:{query.StatusConstraint.Serialize()}";
				return true;
			}
			return false;
		}

		private static void ApplyValidRule(string raw, EditorContentQuery query)
		{
			switch (raw)
			{
				case "y":
				case "valid":
				case "yes":
					query.HasValidationConstraint = true;
					query.ValidationConstraint = ContentValidationStatus.VALID;
					break;
				case "n":
				case "invalid":
				case "no":
					query.HasValidationConstraint = true;
					query.ValidationConstraint = ContentValidationStatus.INVALID;
					break;
			}
		}

		private static void ApplyStatusRule(string raw, EditorContentQuery query)
		{
			query.HasStatusConstraint = false;
			if (ContentModificationStatusExtensions.TryParse(raw, out var status))
			{
				query.HasStatusConstraint = true;
				query.StatusConstraint = status;
			}
		}

		public EditorContentQuery()
		{

		}

		public EditorContentQuery(EditorContentQuery other) : base(other)
		{
			HasValidationConstraint = other?.HasValidationConstraint ?? false;
			ValidationConstraint = other?.ValidationConstraint ?? ContentValidationStatus.VALID;

			HasStatusConstraint = other?.HasStatusConstraint ?? false;
			StatusConstraint = other?.StatusConstraint ?? 0;
		}

		public new static EditorContentQuery Parse(string text)
		{
			return DefaultQueryParser.Parse(text, EditorRules);
		}

		public bool AcceptValidation(ContentValidationStatus status)
		{
			if (!HasValidationConstraint) return true;

			return status == ValidationConstraint;
		}

		public bool AcceptStatus(ContentModificationStatus status)
		{
			if (!HasStatusConstraint) return true;
			return status == (StatusConstraint & status);
		}

		public new string ToString(string existing)
		{
			return DefaultQueryParser.ToString(existing, this, EditorSerializeRules, EditorRules);
		}

		public override string ToString()
		{
			return ToString(null);
		}

		public bool EqualsEditorContentQuery(EditorContentQuery other)
		{
			if (other == null) return false;

			var parentSame = base.Equals(other);
			return parentSame
				   && other.HasStatusConstraint == HasStatusConstraint
				   && other.HasValidationConstraint == HasValidationConstraint
				   && other.StatusConstraint == StatusConstraint
				   && other.ValidationConstraint == ValidationConstraint;
		}
		public override bool Equals(object obj)
		{
			return EqualsEditorContentQuery(obj as EditorContentQuery);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = HasValidationConstraint.GetHashCode();
				hashCode = (hashCode * 397) ^ HasStatusConstraint.GetHashCode();
				hashCode = (hashCode * 397) ^ (int)ValidationConstraint;
				hashCode = (hashCode * 397) ^ (int)StatusConstraint;
				return hashCode;
			}
		}
	}
}
