using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Beamable.Common
{
	[Serializable]
	public class IntTrie : Trie<int, IntTrieEntry> { }

	[Serializable]
	public class IntTrieEntry : TrieSerializationEntry<int> { }

	/// <summary>
	/// A Trie is a tree like data structure that holds information by prefix key.
	///
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public class Trie<T, TEntry> : ISerializationCallbackReceiver
		where TEntry : TrieSerializationEntry<T>, new()
	{
		[DebuggerDisplay("{path} (children=[{children.Count}]) (values=[{values.Count}])")]
		public class Node
		{
			/// <summary>
			/// The full path to this node
			/// </summary>
			public string path;

			/// <summary>
			/// The last part of the path to this node
			/// </summary>
			public string part;

			/// <summary>
			/// The parent <see cref="Node"/> of the current <see cref="Node"/>, or null if no parent exists.
			/// </summary>
			public Node parent;

			/// <summary>
			/// A dictionary where the keys are <see cref="part"/> values, and the values are <see cref="Node"/>s
			/// </summary>
			public Dictionary<string, Node> children = new Dictionary<string, Node>();

			/// <summary>
			/// the values stored at this current <see cref="path"/>
			/// </summary>
			public List<T> values = new List<T>();

			/// <summary>
			/// Iterate through all <see cref="children"/> of the current <see cref="Node"/>, and all the children's children.
			/// This uses a Breath First Search style.
			/// </summary>
			/// <returns></returns>
			public IEnumerable<Node> TraverseChildren()
			{
				var queue = new Queue<Node>();
				foreach (var child in children.Values)
				{
					queue.Enqueue(child);
				}

				while (queue.Count > 0)
				{
					var curr = queue.Dequeue();

					yield return curr;

					foreach (var subChild in curr.children.Values)
					{
						queue.Enqueue(subChild);
					}
				}
			}
		}

		// "a" -> node,
		// doesn't include entires for "a.b"
		private Dictionary<string, Node> _firstPartToNode = new Dictionary<string, Node>();

		// "a.b.c" -> Node
		private Dictionary<string, Node> _pathToNode = new Dictionary<string, Node>();
		private Dictionary<string, List<T>> _pathAllCache = new Dictionary<string, List<T>>();
		private Dictionary<string, List<T>> _pathExactCache = new Dictionary<string, List<T>>();

		[SerializeField]
		private char _splitter = '.';

		[SerializeField]
		private List<TEntry> data = new List<TEntry>();

		public Trie() : this('.')
		{

		}

		public Trie(char splitter)
		{
			_splitter = splitter;
		}

		/// <summary>
		/// Add a value to the given key
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void Insert(string key, T value)
		{
			var node = Search(key);
			node.values.Add(value);
		}

		/// <summary>
		/// Add a series of values to the given key
		/// </summary>
		/// <param name="key"></param>
		/// <param name="values"></param>
		public void InsertRange(string key, IEnumerable<T> values)
		{
			var node = Search(key);
			node.values.AddRange(values);
		}

		/// <summary>
		/// Set the values at the given key to the given values
		/// </summary>
		/// <param name="key"></param>
		/// <param name="values"></param>
		public void SetRange(string key, IEnumerable<T> values)
		{
			var node = Search(key);
			node.values.Clear();
			node.values.AddRange(values);
		}

		/// <summary>
		/// Clear the values at the given path. This will not clear sub values.
		/// </summary>
		/// <param name="key"></param>
		public void ClearExact(string key)
		{
			var node = Search(key);
			node.values.Clear();
		}

		/// <summary>
		/// Remove the given value from the key
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void Remove(string key, T value)
		{
			var node = Search(key);
			node.values.Remove(value);
		}

		/// <summary>
		/// Remove a set of values at the given key
		/// </summary>
		/// <param name="key"></param>
		/// <param name="values"></param>
		public void RemoveRange(string key, IEnumerable<T> values)
		{
			var node = Search(key);
			foreach (var value in values)
			{
				node.values.Remove(value);
			}
		}

		/// <summary>
		/// Get all the values that exist only at the given key.
		/// If the key was "a.b.c", this method will only return the values at "a.b.c",
		/// and it will not return anything at "a.b.c.x".
		/// <para name="x">The returned <see cref="List{T}"/> should not be modified. No modifications will persist in the trie. Instead, use the <see cref="Insert"/>, <see cref="Remove"/>, or <see cref="SetRange"/> methods</para>
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public List<T> GetExact(string key)
		{
			var returnData = new List<T>();
			if (_pathToNode.TryGetValue(key, out var existing))
			{
				returnData.AddRange(existing.values);
			}
			return returnData;
		}

		/// <summary>
		/// Get all values under the given key.
		/// If the given key is "a.b.c", this method will return all values at "a.b.c", and
		/// all values at "a.b.c.x".
		/// <para name="x">The returned <see cref="List{T}"/> should not be modified. No modifications will persist in the trie. Instead, use the <see cref="Insert"/>, <see cref="Remove"/>, or <see cref="SetRange"/> methods</para>
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public List<T> GetAll(IEnumerable<string> keys)
		{
			return GetAll(keys.ToArray());
		}

		/// <summary>
		/// Get all values under the given keys.
		/// If a value appears under multiple keys (such as 'a.b.x' appearing under 'a' and 'a.b'),
		/// then the item will only be returned once.
		/// <para name="x">The returned <see cref="List{T}"/> should not be modified. No modifications will persist in the trie. Instead, use the <see cref="Insert"/>, <see cref="Remove"/>, or <see cref="SetRange"/> methods</para>
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		public List<T> GetAll(params string[] keys)
		{
			var relevantLists = new HashSet<List<T>>();
			foreach (var key in keys)
			{
				if (!_pathAllCache.TryGetValue(key, out var existing))
				{
					_pathAllCache[key] = existing = new List<T>();

					Node last = null;
					foreach (var node in Traverse(key))
					{
						last = node;
					}

					existing.AddRange(last.values);
					foreach (var node in last.TraverseChildren())
					{
						existing.AddRange(node.values);
					}

				}

				relevantLists.Add(existing);
			}

			var output = new List<T>();
			foreach (var list in relevantLists)
			{
				output.AddRange(list);
			}

			return output;
		}

		private void InvalidatePathCache(string key)
		{
			_pathAllCache.Remove(key);
		}

		/// <summary>
		/// Get all current keys for the trie.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetKeys() => _pathToNode.Keys;

		/// <summary>
		/// Given a set of keys, filter the ones that are non-empty in the trie
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		public IEnumerable<string> GetNonEmptyKeys(IEnumerable<string> keys)
		{
			var set = new HashSet<string>();
			foreach (var key in keys)
			{
				if (_pathToNode.TryGetValue(key, out var node) && node.values.Count > 0)
				{
					set.Add(key);
				}
			}
			return set;
		}

		/// <summary>
		/// Given a set of key values, this will return the set of keys that would return data in the trie.
		/// For example, if the trie had data at the following paths,
		/// - a.b.c,
		/// - a.b
		///
		/// And the following <see cref="keys"/> were passed to this method,
		/// - a
		/// - a.c
		/// - a.b
		/// - a.b.c.d
		///
		/// Then only "a" and "a.b" would be returned from the method.
		/// "a.c" does not map to any values currently in the trie, and neither does "a.b.c.d"
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		public HashSet<string> GetRelevantKeys(IEnumerable<string> keys)
		{
			var set = new HashSet<string>();
			foreach (var key in keys)
			{
				var parts = key.Split(_splitter);
				var traversalCount = Traverse(key, false).Count();
				if (parts.Length == traversalCount)
				{
					set.Add(key);
				}
			}

			return set;
		}

		/// <summary>
		/// Iterate through all <see cref="Node"/>s under the given <see cref="key"/>
		/// If the key was "a.b.c", then this would return every child of "a.b.c". 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public IEnumerable<Node> TraverseChildren(string key)
		{
			if (_pathToNode.TryGetValue(key, out var node))
			{
				yield return node;
				foreach (var subNode in node.TraverseChildren())
				{
					yield return subNode;
				}
			}
		}

		/// <summary>
		/// Iterate through the <see cref="Node"/>s along the given <see cref="key"/>
		/// If the given <see cref="key"/> was "a.b.c", then the nodes returned would be
		/// "a", "b", and then "c". 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="autoCreate">If false, then the iteration stops as soon as no node is found the next part of the key</param>
		/// <returns></returns>
		public IEnumerable<Node> Traverse(string key, bool autoCreate = true)
		{
			var parts = key.Split(_splitter);
			var first = parts[0];

			if (!_firstPartToNode.TryGetValue(first, out var node))
			{
				if (!autoCreate)
				{
					yield break;
				}
				_pathToNode[first] = _firstPartToNode[first] = node = new Node
				{
					part = first,
					path = first
				};
			}

			yield return node;
			var subPath = first;
			for (var i = 1; i < parts.Length; i++)
			{
				var curr = parts[i];
				subPath += _splitter + curr;
				if (!node.children.TryGetValue(curr, out var nextNode))
				{
					if (!autoCreate)
					{
						yield break;
					}
					_pathToNode[subPath] = node.children[curr] = nextNode = new Node
					{
						parent = node,
						part = curr,
						path = subPath
					};
				}

				node = nextNode;
				yield return node;
			}

		}

		private Node Search(string key)
		{
			Node last = null;
			foreach (var node in Traverse(key))
			{
				InvalidatePathCache(node.path);
				last = node;
			}

			return last;
		}

		/// <summary>
		/// Before the Trie is serialized, take all of the branches of the tree and collapse them into a single data structure.
		/// </summary>
		public void OnBeforeSerialize()
		{
			data.Clear();
			foreach (var kvp in _pathToNode)
			{
				data.Add(new TEntry
				{
					path = kvp.Key,
					values = kvp.Value.values
				});
			}
		}

		/// <summary>
		/// After the Trie is deserialized, restore the values from <see cref="data"/> into the actual tree structure.
		/// </summary>
		public void OnAfterDeserialize()
		{
			_firstPartToNode.Clear();
			_pathToNode.Clear();
			_pathAllCache.Clear();
			foreach (var entry in data)
			{
				InsertRange(entry.path, entry.values);
			}
		}
	}

	[Serializable]
	public class TrieSerializationEntry<T>
	{
		public string path;
		public List<T> values;
	}
}
