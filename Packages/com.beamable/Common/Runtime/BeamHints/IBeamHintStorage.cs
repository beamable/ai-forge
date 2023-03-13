using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace Beamable.Common.Assistant
{
	/// <summary>
	/// Defines a storage for <see cref="BeamHint"/>s. It is a query-able in-memory database of <see cref="BeamHint"/>s.
	/// Other <see cref="IBeamHintSystem"/> systems add hints to these and <see cref="IBeamHintSystem"/> read, filter, clear and arrange data logically in
	/// relation to <see cref="BeamHintHeader"/>s to be read by UI. 
	/// </summary>
	public interface IBeamHintStorage : IEnumerable<BeamHint>
	{
		/// <summary>
		/// Adds a hint to the storage.
		/// </summary>
		/// <param name="type">The type of hint that it is.</param>
		/// <param name="originSystem">The system that originated this hint.</param>
		/// <param name="hintDomain">An arbitrary contextual grouping for the hint.</param>
		/// <param name="uniqueId">An id, unique when combined with <paramref name="hintDomain"/>, that identifies the hint.</param>
		/// <param name="hintContextObj">Any arbitrary data that you wish to tie to the hint.</param>
		void AddOrReplaceHint(BeamHintType type, string hintDomain, string uniqueId, object hintContextObj = null);

		/// <summary>
		/// Adds a hint to the storage.
		/// </summary>
		/// <param name="header">A pre-built <see cref="BeamHintHeader"/> to add.</param>
		/// <param name="hintContextObj">Any arbitrary data that you wish to tie to the hint.</param>
		void AddOrReplaceHint(BeamHintHeader header, object hintContextObj = null);

		/// <summary>
		/// Takes in two parallel <see cref="IEnumerable{T}"/> (same-length arrays) of <see cref="BeamHintHeader"/>/<see cref="object"/> pairs and add them to the storage.
		/// </summary>
		void AddOrReplaceHints(IEnumerable<BeamHintHeader> headers, IEnumerable<object> hintContextObjs);

		/// <summary>
		/// Adds the given <see cref="BeamHint"/>s.
		/// </summary>
		void AddOrReplaceHints(IEnumerable<BeamHint> bakedHints);

		/// <summary>
		/// Removes the <see cref="BeamHint"/> identified by the <paramref name="header"/>.
		/// </summary>
		void RemoveHint(BeamHintHeader header);

		/// <summary>
		/// Removes the given <paramref name="hint"/> from the storage.
		/// </summary>
		void RemoveHint(BeamHint hint);

		/// <summary>
		/// Removes the <see cref="BeamHint"/>s identified by the given <paramref name="headers"/> from the storage.
		/// </summary>
		void RemoveHints(IEnumerable<BeamHintHeader> headers);

		/// <summary>
		/// Removes the given <paramref name="hints"/> from the storage.
		/// </summary>
		void RemoveHints(IEnumerable<BeamHint> hints);

		/// <summary>
		/// Removes all hints that <see cref="Regex.Match(string)"/> of any of the given <paramref name="hintDomains"/> and <paramref name="hintIds"/>.
		/// </summary>
		int RemoveAllHints(IEnumerable<string> hintDomains, IEnumerable<string> hintIds);

		/// <summary>
		/// Remove all hints of the given <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The <see cref="BeamHintType"/>s to remove.</param>
		/// <returns>The amount of <see cref="BeamHint"/>s removed.</returns>
		int RemoveAllHints(BeamHintType type);

		/// <summary>
		/// Removes all hints that <see cref="Regex.Match(string)"/> of the given <paramref name="hintDomainRegex"/> and <paramref name="idRegex"/>.
		/// </summary>
		int RemoveAllHints(string hintDomainRegex = ".*", string idRegex = ".*");

		/// <summary>
		/// Given a <paramref name="header"/>, returns a <see cref="BeamHint"/> containing it's associated <see cref="BeamHint.ContextObject"/>.
		/// </summary>
		BeamHint GetHint(BeamHintHeader header);
	}

	/// <summary>
	/// Defines a storage for <see cref="BeamHint"/>s. This implementation is header-based, meaning:
	/// <para/>
	/// - It contains a <see cref="HashSet{T}"/> of unique headers for each added hint. Hints are considered existing if they are present in the <see cref="_headers"/> set.
	/// <para/>
	/// - It contains a <see cref="Dictionary{TKey,TValue}"/>, indexed by <see cref="BeamHintHeader"/>, holding each hint's context object. The key exists even if the object is null.
	/// <para/>
	/// - Queries are made over the <see cref="HashSet{T}"/> of headers and returned in <see cref="BeamHint"/> format.
	/// </summary>
	public class BeamHintStorage : IBeamHintStorage
	{
		private HashSet<BeamHintHeader> _headers;
		private Dictionary<BeamHintHeader, object> _hintContextObjects;

		/// <summary>
		/// Creates a new <see cref="BeamHintStorage"/> instance. Makes no assumptions with respect to what types of hints will be stored here.
		/// </summary>
		public BeamHintStorage()
		{
			_headers = new HashSet<BeamHintHeader>();
			_hintContextObjects = new Dictionary<BeamHintHeader, object>();
		}


		#region IEnumerable Implementation

		/// <summary>
		/// Allows iteration over storage with for-each and use of LINQ methods directly over BeamHints.
		/// </summary>
		public IEnumerator<BeamHint> GetEnumerator()
		{
			return _headers.Select(h => new BeamHint(h, _hintContextObjects[h])).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		public void AddOrReplaceHint(BeamHintType type, string hintDomain, string uniqueId, object hintContextObj = null)
		{
			var header = new BeamHintHeader(type, hintDomain, uniqueId);
			AddOrReplaceHint(header, hintContextObj);
		}

		public void AddOrReplaceHint(BeamHintHeader header, object hintContextObj = null)
		{
			if (!_headers.Contains(header))
				_headers.Add(header);

			if (_hintContextObjects.ContainsKey(header))
				_hintContextObjects[header] = hintContextObj;
			else
				_hintContextObjects.Add(header, hintContextObj);
		}

		public void AddOrReplaceHints(IEnumerable<BeamHintHeader> headers, IEnumerable<object> hintContextObjs)
		{
			var contextObjs = hintContextObjs.ToList();
			var beamHint_headers = headers.ToList();

			System.Diagnostics.Debug.Assert(beamHint_headers.Count == contextObjs.Count, "These must be parallel arrays of the same length.");

			var zipped = beamHint_headers.Zip(contextObjs, (header, obj) => new { Header = header, ContextObject = obj });
			foreach (var hint in zipped)
			{
				AddOrReplaceHint(hint.Header, hint.ContextObject);
			}
		}

		public void AddOrReplaceHints(IEnumerable<BeamHint> bakedHints)
		{
			foreach (var hint in bakedHints)
			{
				AddOrReplaceHint(hint.Header, hint.ContextObject);
			}
		}

		public void RemoveHint(BeamHintHeader header)
		{
			RemoveHints(new[] { header });
		}

		public void RemoveHint(BeamHint hint)
		{
			RemoveHints(new[] { hint });
		}

		public void RemoveHints(IEnumerable<BeamHintHeader> headers)
		{
			foreach (var toRemove in headers)
			{
				_headers.Remove(toRemove);
			}
		}

		public void RemoveHints(IEnumerable<BeamHint> hints)
		{
			RemoveHints(hints.Select(h => h.Header));
		}

		public int RemoveAllHints(IEnumerable<string> hintDomains, IEnumerable<string> hintIds)
		{
			var hintDomainRegexStr = string.Join("|", hintDomains);
			var hintIdRegexStr = string.Join("|", hintIds);

			return RemoveAllHints(hintDomainRegexStr, hintIdRegexStr);
		}

		public int RemoveAllHints(BeamHintType type)
		{
			var removedCount = _headers.RemoveWhere((header => (header.Type & type) != 0));
			return removedCount;
		}

		public int RemoveAllHints(string hintDomainRegex = ".*", string idRegex = ".*")
		{
			if (hintDomainRegex == ".*" && idRegex == ".*")
			{
				_headers.Clear();
			}

			var domainReg = new Regex(hintDomainRegex);
			var idReg = new Regex(idRegex);

			var removedCount = _headers.RemoveWhere(header => (domainReg.Match(header.Domain).Success && idReg.Match(header.Id).Success));
			return removedCount;
		}

		public BeamHint GetHint(BeamHintHeader header)
		{
			if (_hintContextObjects.TryGetValue(header, out var obj))
				return new BeamHint(header, obj);

			throw new ArgumentException($"The given {header} was not found!");
		}
	}
}
