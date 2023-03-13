using Beamable.Common;
using Beamable.Common.Content;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Content
{
	public struct ContentDatabaseEntry
	{
		/// <summary>
		/// Where is the asset for this content?
		/// </summary>
		public string assetPath;

		/// <summary>
		/// The content id, where the id is a `.` deliminated string.
		/// The final clause is the <see cref="contentName"/> , and everything else makes up the <see cref="contentType"/>
		/// </summary>
		public string contentId;

		/// <summary>
		/// The name of the content, which is the final right-most clause in the <see cref="contentId"/>
		/// </summary>
		public string contentName;

		/// <summary>
		/// the type of the content, which is everything in the <see cref="contentId"/> except for the right-most clause.
		/// </summary>
		public string contentType;

		/// <summary>
		/// the Type of this content
		/// </summary>
		public Type runtimeType;
	}


	public class ContentDatabase
	{
		private class ContentTypeNode
		{
			public string type;
			public ContentTypeNode parent;
			public List<ContentTypeNode> children = new List<ContentTypeNode>();
		}

		private ContentTypeReflectionCache _typeCache;
		private List<ContentDatabaseEntry> _data;
		private Dictionary<string, ContentDatabaseEntry> _idToContent = new Dictionary<string, ContentDatabaseEntry>();

		private Dictionary<string, List<ContentDatabaseEntry>> _typeToExactContent =
			new Dictionary<string, List<ContentDatabaseEntry>>();

		private Dictionary<string, List<List<ContentDatabaseEntry>>> _assignableContent = new Dictionary<string, List<List<ContentDatabaseEntry>>>();
		private Dictionary<string, List<ContentDatabaseEntry>> _assignableContentFlat = new Dictionary<string, List<ContentDatabaseEntry>>();
		private Dictionary<string, ContentTypeNode> _typeToNode = new Dictionary<string, ContentTypeNode>();
		private Dictionary<string, ContentDatabaseEntry[]> _assignableContentFlatArray =
			new Dictionary<string, ContentDatabaseEntry[]>();

		private Dictionary<string, ContentDatabaseEntry>
			_pathToContent = new Dictionary<string, ContentDatabaseEntry>();


		public ContentDatabase()
		{
			_typeCache = BeamEditor.GetReflectionSystem<ContentTypeReflectionCache>();

			RecalculateIndex();
		}

		/// <summary>
		/// Updates the database so that any future calls will return accurate data.
		/// This method scans the entire content directory and builds up metadata about all of the content.
		/// This method is called once at startup, but it must be called as the editor lifecycle continues, or the
		/// data will be stale. However, if this method is called too often, it could lead to performance issues. 
		/// </summary>
		public void RecalculateIndex()
		{

			#region clear old data
			_typeToExactContent.Clear();
			_assignableContent.Clear();
			_assignableContentFlat.Clear();
			_typeToNode.Clear();
			_assignableContentFlatArray.Clear();
			_idToContent.Clear();
			_pathToContent.Clear();
			#endregion

			#region initialize variables and setup alg
			var root = Constants.Directories.DATA_DIR;
			var filePathsToExpand = new Stack<string>();
			var typeString = new Stack<string>();
			var typeToContentList = new Stack<List<ContentDatabaseEntry>>();
			var nodeStack = new Stack<ContentTypeNode>();
			filePathsToExpand.Push(root);

			_data = new List<ContentDatabaseEntry>();

			string currentFilePath = null;
			string currType = null;
			List<ContentDatabaseEntry> currList = null;
			ContentTypeNode currNode = null;
			ContentTypeNode rootNode = new ContentTypeNode();

			nodeStack.Push(rootNode);
			#endregion

			#region DFS over file structure
			while (filePathsToExpand.Count > 0)
			{
				currentFilePath = filePathsToExpand.Pop();

				var hasCurrType = BeamableStackTryPop(typeString, out currType);
				var hasContentList = BeamableStackTryPop(typeToContentList, out currList);
				var hasParent = BeamableStackTryPop(nodeStack, out currNode);

				Type runtimeType = null;
				if (hasCurrType)
				{
					_typeCache.TryGetType(currType, out runtimeType);
				}

				if (!Directory.Exists(currentFilePath)) continue; // if this directory doesn't exist, we can't do anything.

				if (hasContentList)
				{
					foreach (var filePath in Directory.GetFiles(currentFilePath, "*.asset"))
					{
						var instance = new ContentDatabaseEntry();
						var name = filePath.Substring(currentFilePath.Length + 1, filePath.Length - (currentFilePath.Length + ".asset".Length + 1));

						instance.contentName = name;
						instance.assetPath = filePath;
						instance.contentType = currType;
						instance.runtimeType = runtimeType;
						instance.contentId = currType + "." + name;
						_data.Add(instance);
						currList.Add(instance);
					}
				}

				foreach (var path in Directory.GetDirectories(currentFilePath))
				{
					var type = path.Substring(currentFilePath.Length + 1, path.Length - (currentFilePath.Length + 1));
					if (hasCurrType)
					{
						type = currType + "." + type;
					}
					typeString.Push(type);
					filePathsToExpand.Push(path);

					var nextContentList = new List<ContentDatabaseEntry>();
					typeToContentList.Push(nextContentList);
					_typeToExactContent[type] = nextContentList;

					var nextNode = new ContentTypeNode();
					nextNode.parent = currNode;
					currNode.children.Add(nextNode);
					nextNode.type = type;
					nodeStack.Push(nextNode);

					_assignableContent[type] = new List<List<ContentDatabaseEntry>>
					{
						nextContentList
					};
					_typeToNode[type] = nextNode;

					var p = nextNode.parent;
					while (p != null && p.type != null)
					{
						if (_assignableContent.TryGetValue(p.type, out var set))
						{
							set.Add(nextContentList);
						}
						p = p.parent;
					}
				}
			}
			#endregion

			#region data formating
			foreach (var kvp in _assignableContent)
			{
				_assignableContentFlat[kvp.Key] = new List<ContentDatabaseEntry>();
				foreach (var list in kvp.Value)
				{
					_assignableContentFlat[kvp.Key].AddRange(list);
				}
			}

			foreach (var kvp in _assignableContentFlat)
			{
				_assignableContentFlatArray[kvp.Key] = kvp.Value.ToArray();
			}

			foreach (var elem in _data)
			{
				_idToContent[elem.contentId] = elem;
				_pathToContent[elem.assetPath] = elem;
			}

			#endregion

		}

		private static bool BeamableStackTryPop<T>(Stack<T> stack, out T value)
		{
			if (stack.Count > 0)
			{
				value = stack.Pop();
				return true;
			}

			value = default(T);
			return false;
		}

		/// <summary>
		/// Returns true if any of the given paths start with the content data directory path.
		/// This method is useful to check if any paths from an AssetDatabase relate to content at all. 
		/// </summary>
		/// <param name="paths">An array of file paths</param>
		/// <returns>true if any of the given paths start with the content data directory.</returns>
		public bool ContainsAnyContentPaths(string[] paths)
		{
			for (var i = 0; i < paths.Length; i++)
			{
				if (paths[i].StartsWith(Constants.Directories.DATA_DIR))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Given an asset path, try to get the <see cref="ContentDatabaseEntry"/> for the content at the path.
		/// If assets have been modified, this function may return stale data. Consider running the <see cref="RecalculateIndex"/> method before this.
		/// </summary>
		/// <param name="path">Some path where content may be</param>
		/// <param name="entry">The output <see cref="ContentDatabaseEntry"/> that describes the asset.</param>
		/// <returns>true if the <see cref="entry"/> was found, false otherwise.</returns>
		public bool TryGetContentByPath(string path, out ContentDatabaseEntry entry)
		{
			return _pathToContent.TryGetValue(path, out entry);
		}

		/// <summary>
		/// Given a content id, try to get the <see cref="ContentDatabaseEntry"/> for the id.
		/// If assets have been modified, this function may return stale data. Consider running the <see cref="RecalculateIndex"/> method before this.
		/// </summary>
		/// <param name="id">Some content id</param>
		/// <param name="entry">The output <see cref="ContentDatabaseEntry"/> that describes the asset.</param>
		/// <returns>true if the <see cref="entry"/> was found, false otherwise.</returns>
		public bool TryGetContentById(string id, out ContentDatabaseEntry entry)
		{
			return _idToContent.TryGetValue(id, out entry);
		}

		/// <summary>
		/// Get all <see cref="ContentDatabaseEntry"/>s for the given type, T.
		/// This method will return entries that are sub types of the content type as well.
		/// If assets have been modified, this function may return stale data. Consider running the <see cref="RecalculateIndex"/> method before this.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>An array of <see cref="ContentDatabaseEntry"/></returns>
		public ContentDatabaseEntry[] GetContent<T>() where T : ContentObject => GetContent(typeof(T));

		/// <inheritdoc cref="GetContent{T}"/>
		public ContentDatabaseEntry[] GetContent(Type t)
		{
			var id = _typeCache.ClassToContentType[t];
			return _assignableContentFlatArray[id];
		}

		/// <summary>
		/// Get all <see cref="ContentDatabaseEntry"/>s
		/// If assets have been modified, this function may return stale data. Consider running the <see cref="RecalculateIndex"/> method before this.
		/// </summary>
		/// <returns>An array of <see cref="ContentDatabaseEntry"/></returns>
		public List<ContentDatabaseEntry> GetAllContent() => _data;
	}
}
