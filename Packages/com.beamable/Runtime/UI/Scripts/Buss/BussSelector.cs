using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Beamable.UI.Buss
{
	public abstract class BussSelector
	{
		public abstract bool CheckMatch(BussElement bussElement);
		public abstract SelectorWeight GetWeight();

		public virtual bool TryGetPseudoClass(out string pseudoClass)
		{
			pseudoClass = null;
			return false;
		}

		/// <summary>
		/// Similar to <see cref="CheckMatch"/>, except that this method will
		/// return true if the selector matches the given element, or ANY element in the given element's parent lineage.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public bool IsElementIncludedInSelector(BussElement element)
		{
			return IsElementIncludedInSelector(element, out _, out _);
		}

		public bool IsElementIncludedInSelector(BussElement element, out bool isExactMatch)
		{
			return IsElementIncludedInSelector(element, out isExactMatch, out _);
		}

		public bool IsElementIncludedInSelector(BussElement element, out bool isExactMatch, out int parentDistance)
		{
			isExactMatch = false;
			parentDistance = 0;
			if (element == null) return false;

			var maxDepth = 100;
			while (maxDepth-- > 0 && element != null)
			{
				var isMatch = CheckMatch(element);
				if (isMatch)
				{
					isExactMatch = parentDistance == 0;
					return true;
				}

				parentDistance++;
				element = element?.Parent;
			}
			return false;
		}

	}

	/// <summary>
	/// Class that describes BUSS selectors weight
	/// <param name="idCount">Amount of ids</param>
	/// <param name="classCount">Amount of classes, attributes and pseudo-classes</param>
	/// <param name="elementCount">Amount of elements and pseudo-elements</param>
	/// </summary>
	public struct SelectorWeight : IComparable<SelectorWeight>
	{
		public static readonly SelectorWeight Max = new SelectorWeight(int.MaxValue, int.MaxValue, int.MaxValue);
		public static readonly SelectorWeight Min = new SelectorWeight(int.MinValue, int.MinValue, int.MinValue);

		public int IdCount { get; }
		public int ClassCount { get; }
		public int ElementCount { get; }

		public SelectorWeight(int idCount, int classCount, int elementCount)
		{
			IdCount = idCount;
			ClassCount = classCount;
			ElementCount = elementCount;
		}

		public static SelectorWeight operator +(SelectorWeight x, SelectorWeight y)
		{
			int idCount = x.IdCount + y.IdCount;
			int classCount = x.ClassCount + y.ClassCount;
			int elementsCount = x.ElementCount + y.ElementCount;

			return new SelectorWeight(idCount, classCount, elementsCount);
		}

		public int CompareTo(SelectorWeight other)
		{
			int idCountComparison = IdCount.CompareTo(other.IdCount);
			if (idCountComparison != 0) return idCountComparison;
			int classCountComparison = ClassCount.CompareTo(other.ClassCount);
			if (classCountComparison != 0) return classCountComparison;
			return ElementCount.CompareTo(other.ElementCount);
		}

		public override string ToString()
		{
			return $"Id: {IdCount}, class: {ClassCount}, element: {ElementCount}";
		}
	}

	public class UniversalSelector : BussSelector
	{
		public static UniversalSelector Get { get; } = new UniversalSelector();

		private UniversalSelector() { }

		public override bool CheckMatch(BussElement bussElement)
		{
			return bussElement != null;
		}

		public override SelectorWeight GetWeight()
		{
			return new SelectorWeight(0, 0, 0);
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public override bool Equals(object obj)
		{
			return obj is UniversalSelector;
		}
	}

	public class IdSelector : BussSelector
	{
		public readonly string Id;

		private IdSelector(string id)
		{
			Id = id;
		}

		private static readonly Dictionary<string, IdSelector> _idSelectors = new Dictionary<string, IdSelector>();

		public static IdSelector Get(string id)
		{
			if (!_idSelectors.TryGetValue(id, out var selector))
			{
				_idSelectors[id] = selector = new IdSelector(id);
			}

			return selector;
		}

		public override bool CheckMatch(BussElement bussElement)
		{
			if (bussElement == null) return false;
			return bussElement.Id == Id;
		}

		public override SelectorWeight GetWeight()
		{
			return new SelectorWeight(1, 0, 0);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}

	public class TypeSelector : BussSelector
	{
		public readonly string TypeName;

		private TypeSelector(string typeName)
		{
			TypeName = typeName;
		}

		private static readonly Dictionary<string, TypeSelector>
			_typeSelectors = new Dictionary<string, TypeSelector>();

		public static TypeSelector Get(string typeName)
		{
			if (!_typeSelectors.TryGetValue(typeName, out var selector))
			{
				_typeSelectors[typeName] = selector = new TypeSelector(typeName);
			}

			return selector;
		}

		public override bool CheckMatch(BussElement bussElement)
		{
			if (bussElement == null) return false;
			return bussElement.TypeName == TypeName;
		}

		public override SelectorWeight GetWeight()
		{
			return new SelectorWeight(0, 0, 1);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return TypeName.GetHashCode() + 2;
			}
		}
	}

	public class ClassSelector : BussSelector
	{
		public readonly string ClassName;

		private ClassSelector(string className)
		{
			ClassName = className;
		}

		private static readonly Dictionary<string, ClassSelector> _classSelectors =
			new Dictionary<string, ClassSelector>();

		public static ClassSelector Get(string className)
		{
			if (!_classSelectors.TryGetValue(className, out var selector))
			{
				_classSelectors[className] = selector = new ClassSelector(className);
			}

			return selector;
		}

		public override bool CheckMatch(BussElement bussElement)
		{
			if (bussElement == null) return false;
			return bussElement.Classes.Contains(ClassName);
		}

		public override SelectorWeight GetWeight()
		{
			return new SelectorWeight(0, 1, 0);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ClassName.GetHashCode() + 1;
			}
		}
	}

	public class PseudoSelector : BussSelector
	{
		public readonly string PseudoName;

		private PseudoSelector(string pseudoName)
		{
			PseudoName = pseudoName;
		}

		private static readonly Dictionary<string, PseudoSelector> _pseudoSelectors =
			new Dictionary<string, PseudoSelector>();

		public static PseudoSelector Get(string pseudoName)
		{
			if (!_pseudoSelectors.TryGetValue(pseudoName, out var selector))
			{
				_pseudoSelectors[pseudoName] = selector = new PseudoSelector(pseudoName);
			}

			return selector;
		}

		public override bool CheckMatch(BussElement bussElement)
		{
			return bussElement != null;
		}

		public override SelectorWeight GetWeight()
		{
			return new SelectorWeight(0, 1, 0);
		}

		public override bool TryGetPseudoClass(out string pseudoClass)
		{
			pseudoClass = PseudoName;
			return true;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return PseudoName.GetHashCode() + 3;
			}
		}
	}

	public class CombinedSelector : BussSelector
	{
		public readonly BussSelector[] Selectors;
		public readonly bool RequireAll;

		public CombinedSelector(BussSelector[] selectors, bool requireAll)
		{
			Selectors = selectors;
			RequireAll = requireAll;
		}

		public override bool CheckMatch(BussElement bussElement)
		{
			if (bussElement == null) return false;
			if (RequireAll)
			{
				foreach (var selector in Selectors)
				{
					if (!selector.CheckMatch(bussElement))
					{
						return false;
					}
				}

				return true;
			}

			foreach (var selector in Selectors)
			{
				if (selector.CheckMatch(bussElement))
				{
					return true;
				}
			}

			return false;
		}

		public override SelectorWeight GetWeight()
		{
			SelectorWeight selectorWeight = new SelectorWeight(0, 0, 0);

			foreach (BussSelector bussSelector in Selectors)
			{
				selectorWeight += bussSelector.GetWeight();
			}

			return selectorWeight;
		}
	}

	public class ParentedSelector : BussSelector
	{
		public readonly BussSelector BaseSelector;
		public readonly BussSelector ParentSelector;
		public readonly bool OnlyDirectParent;

		public ParentedSelector(BussSelector baseSelector, BussSelector parentSelector, bool onlyDirectParent)
		{
			ParentSelector = parentSelector;
			OnlyDirectParent = onlyDirectParent;
			BaseSelector = baseSelector;
		}

		public override bool CheckMatch(BussElement bussElement)
		{
			if (bussElement == null) return false;
			if (BaseSelector.CheckMatch(bussElement))
			{
				if (OnlyDirectParent)
				{
					return ParentSelector.CheckMatch(bussElement.Parent);
				}
				else
				{
					while (bussElement.Parent != null)
					{
						bussElement = bussElement.Parent;
						if (ParentSelector.CheckMatch(bussElement))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public override SelectorWeight GetWeight()
		{
			return ParentSelector.GetWeight() + BaseSelector.GetWeight();
		}
	}

	public static class BussSelectorParser
	{
		public static readonly Regex IdRegex = new Regex("#[a-zA-Z0-9-_]+");
		public static readonly Regex ClassRegex = new Regex("\\.[a-zA-Z0-9-_]+");
		public static readonly Regex TypeRegex = new Regex("^[a-zA-Z]+[a-zA-Z0-9_]*");
		public static readonly Regex PseudoRegex = new Regex("\\:[a-zA-Z0-9-_]+");

		private static readonly Dictionary<string, BussSelector> ParsedSelectors = new Dictionary<string, BussSelector>();

		public static BussSelector Parse(string input)
		{
			if (ParsedSelectors.TryGetValue(input, out var cached))
			{
				return cached;
			}

			BussSelector result = null;

			var separation = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			var selectors = new List<BussSelector>();
			foreach (var part in separation)
			{
				var selector = TryParseSingle(part);
				if (selector != null)
				{
					selectors.Add(selector);
				}
			}

			if (selectors.Count > 1)
			{
				result = new CombinedSelector(selectors.ToArray(), false);
			}
			else if (selectors.Count > 0)
			{
				result = selectors[0];
			}

			ParsedSelectors[input] = result;

			return result;
		}

		private static BussSelector TryParseSingle(string input)
		{
			var separation = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			BussSelector parent = null;
			bool onlyDirectParenting = false;
			for (int i = 0; i < separation.Length; i++)
			{
				if (separation[i] == ">")
				{
					onlyDirectParenting = true;
					continue;
				}

				var selector = TryParseSimple(separation[i]);
				if (selector != null)
				{
					parent = parent == null ? selector : new ParentedSelector(selector, parent, onlyDirectParenting);

					onlyDirectParenting = false;
				}
			}

			return parent;
		}

		private static BussSelector TryParseSimple(string input)
		{
			input = input.Trim();

			if (input == "*")
			{
				return UniversalSelector.Get;
			}

			var selectors = new List<BussSelector>();

			var typeMatch = TypeRegex.Match(input);
			if (typeMatch.Success)
			{
				selectors.Add(TypeSelector.Get(typeMatch.Value));
			}

			var idMatch = IdRegex.Match(input);
			if (idMatch.Success)
			{
				selectors.Add(IdSelector.Get(idMatch.Value.Substring(1)));
			}

			var classMatches = ClassRegex.Matches(input);
			for (int i = 0; i < classMatches.Count; i++)
			{
				var match = classMatches[i];
				selectors.Add(ClassSelector.Get(match.Value.Substring(1)));
			}

			var pseudoMatches = PseudoRegex.Matches(input);
			for (int i = 0; i < pseudoMatches.Count; i++)
			{
				var match = pseudoMatches[i];
				selectors.Add(PseudoSelector.Get(match.Value.Substring(1)));
			}

			if (selectors.Count > 1)
			{
				return new CombinedSelector(selectors.ToArray(), true);
			}

			if (selectors.Count > 0)
			{
				return selectors[0];
			}

			return null;
		}
	}
}
