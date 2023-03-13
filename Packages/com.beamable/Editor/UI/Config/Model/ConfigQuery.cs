using Beamable.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Editor.Config.Model
{
	public class ConfigQuery : DefaultQuery
	{
		public HashSet<string> ModuleConstraint;

		protected static Dictionary<string, DefaultQueryParser.ApplyParseRule<ConfigQuery>> EditorRules;
		protected static List<DefaultQueryParser.SerializeRule<ConfigQuery>> EditorSerializeRules;
		static ConfigQuery()
		{
			EditorRules = new Dictionary<string, DefaultQueryParser.ApplyParseRule<ConfigQuery>>
		 {
			{"module", ApplyModuleRule},
			{"id", DefaultQueryParser.ApplyIdParse},
		 };

			EditorSerializeRules = new List<DefaultQueryParser.SerializeRule<ConfigQuery>>
		 {
			SerializeModuleRule, SerializeIdRule
		 };
		}

		public ConfigQuery()
		{

		}
		public ConfigQuery(ConfigQuery other)
		{
			if (other == null) return;

			ModuleConstraint = other.ModuleConstraint != null
			   ? new HashSet<string>(other.ModuleConstraint.ToArray())
			   : null;
			IdContainsConstraint = other.IdContainsConstraint;
		}

		protected static bool SerializeModuleRule(ConfigQuery query, out string str)
		{
			str = "";
			if (query.ModuleConstraint == null)
			{
				return false;
			}
			str = $"module:{string.Join(" ", query.ModuleConstraint)}";
			return true;
		}

		private static void ApplyModuleRule(string raw, ConfigQuery query)
		{
			var modules = raw.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			query.ModuleConstraint = new HashSet<string>(modules);
		}

		public string ToString(string existing)
		{
			return DefaultQueryParser.ToString(existing, this, EditorSerializeRules, EditorRules);
		}

		public override string ToString()
		{
			return ToString(null);
		}

		public static ConfigQuery Parse(string text)
		{
			return DefaultQueryParser.Parse(text, EditorRules);
		}

		public bool EqualsConfigQuery(ConfigQuery other)
		{
			if (other == null) return false;

			var parentSame = base.Equals(other);
			return parentSame
				   && other.IdContainsConstraint == IdContainsConstraint
				   && other.ModuleConstraint == ModuleConstraint;
		}
		public override bool Equals(object obj)
		{
			return EqualsConfigQuery(obj as ConfigQuery);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = IdContainsConstraint.GetHashCode();
				hashCode = (hashCode * 397) ^ ModuleConstraint.GetHashCode();
				return hashCode;
			}
		}

		public bool Accepts(ConfigOption option)
		{
			return AcceptIdContains(option.Name.ToLower()) && AcceptModule(option);
		}

		public bool AcceptModule(ConfigOption config)
		{
			if (ModuleConstraint == null) return true;
			if (config == null)
			{
				return ModuleConstraint.Count == 0;
			}

			return ModuleConstraint.Contains(config.Module);
		}
	}
}
