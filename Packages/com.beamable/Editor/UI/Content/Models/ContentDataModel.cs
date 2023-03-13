using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Editor.Content.Extensions;
using Beamable.Editor.Content.Helpers;
using Beamable.Editor.Content.SaveRequest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Beamable.Editor.Content.Models
{
	public class ContentDataModel
	{
		/// <summary>
		/// Toggle false for production.
		/// </summary>
		private static bool IsDebugging = false; //Keep "static" to limit warnings

		public event Action<IList<ContentItemDescriptor>> OnSelectedContentChanged;
		public event Action<IList<TreeViewItem>> OnSelectedContentTypesChanged;
		public event Action<IList<TreeViewItem>> OnSelectedContentTypeBranchChanged;

		public event Action<ContentItemDescriptor> OnItemEnriched;
		public event Action OnFilterChanged;
		public event Action OnFilteredContentsChanged;
		public event Action<EditorContentQuery, bool> OnQueryUpdated;
		public event Action<ContentItemDescriptor> OnContentAdded;
		public event Action<ContentItemDescriptor> OnContentDeleted;
		public event Action<ContentTypeDescriptor> OnTypeAdded;
		public event Action OnTypesReceived;
		public event Action<bool> OnUserCanPublishChanged;

		public event Action OnSoftReset;
		public event Action OnManifestChanged;

		public ContentSortType CurrentSorter { get; private set; } = ContentSortType.IdAZ;
		public EditorContentQuery Filter { get; private set; }
		public EditorContentQuery SystemFilter { get; private set; } = new EditorContentQuery();

		private bool _userCanPublish = false;

		public bool UserCanPublish
		{
			get => _userCanPublish;
			set
			{
				_userCanPublish = value;
				OnUserCanPublishChanged?.Invoke(value);
			}
		}

		public long LastManifestTimestamp => ServerManifest?.Created ?? 0;
		public int TotalContentCount => _content.Count;

		private Dictionary<string, ManifestReferenceSuperset> _lastServerManifest = new Dictionary<string, ManifestReferenceSuperset>();
		private List<ContentItemDescriptor> _content = new List<ContentItemDescriptor>();
		private List<ContentItemDescriptor> _filteredContent = new List<ContentItemDescriptor>();
		private Dictionary<string, ContentItemDescriptor> _idToContent = new Dictionary<string, ContentItemDescriptor>();
		private Dictionary<string, ContentTypeDescriptor> _nameToType = new Dictionary<string, ContentTypeDescriptor>();
		private Dictionary<string, ContentTagDescriptor> _tagToDescriptor = new Dictionary<string, ContentTagDescriptor>();
		public ContentIO ContentIO { get; set; }

		public ContentDataModel()
		{
			ContentIO.OnManifestChanged += ManifestChanged;
		}

		public ContentDataModel(ContentIO contentIO)
		{
			ContentIO = contentIO;
			ContentIO.OnManifestChanged += ManifestChanged;
		}

		private void ManifestChanged(string manifestId)
		{
			OnManifestChanged?.Invoke();
		}

		public void TriggerSoftReset()
		{
			OnSoftReset?.Invoke();
		}

		public void SetContentTypes(List<ContentTypePair> types)
		{
			ContentTypePairs = types;
			var unprocessedTypeNames = new HashSet<string>(_nameToType.Keys);
			foreach (var type in types)
			{
				unprocessedTypeNames.Remove(type.Name);

				if (_nameToType.TryGetValue(type.Name, out var existingType))
				{
					existingType.EnrichWithLocal(type.Type);
				}
				else
				{
					var typeDesc = new ContentTypeDescriptor();
					typeDesc.SetFromLocal(type.Name, type.Type);
					AddType(typeDesc);

				}
			}

			foreach (var unprocessedType in unprocessedTypeNames)
			{
				// TODO what happens with content items that reference this type?
				_nameToType.Remove(unprocessedType);
			}

			OnTypesReceived?.Invoke();
		}

		public void CreateItem(ContentObject content)
		{
			ContentIO.Create(content, null, false, LocalManifest);
		}

		public void DeleteItems(List<ContentItemDescriptor> descriptors)
		{
			foreach (var desc in descriptors)
			{
				if (SelectedContents.Contains(desc))
				{
					SelectedContents.Remove(desc);
				}
			}

			var contents = descriptors.Select(d => d.GetContent()).ToList();
			ContentIO.DeleteBatch(contents);

		}

		[Obsolete("Do not use. Use " + nameof(DeleteItems) + " instead.")]
		public void DeleteItem(ContentItemDescriptor contentItemDescriptor)
		{
			DeleteItems(new List<ContentItemDescriptor>() { contentItemDescriptor });
		}

		public void DeleteLocalOnlyItems()
		{
			var toDelete = _content.Where(c => c.Status == ContentModificationStatus.LOCAL_ONLY).ToList();//.ForEach(DeleteItem);
			DeleteItems(toDelete);
		}

		public void SetLocalContent(LocalContentManifest localManifest)
		{
			LocalManifest = localManifest;
			_content.Clear();
			_idToContent.Clear();

			foreach (var kvp in localManifest.Content)
			{
				// build the type information first.

				if (_nameToType.TryGetValue(kvp.Value.TypeName, out var existingTypeDesc))
				{
					existingTypeDesc.EnrichWithLocal(kvp.Value.ContentType);
				}
				else
				{
					var newTypeDesc = new ContentTypeDescriptor();
					newTypeDesc.SetFromLocal(kvp.Value);
					AddType(newTypeDesc);
				}


				if (_idToContent.TryGetValue(kvp.Key, out var existing))
				{
					existing.EnrichWithLocalData(kvp.Value);
					AccumulateContentTags(existing);
				}
				else
				{
					var item = new ContentItemDescriptor(kvp.Value, _nameToType[kvp.Value.TypeName]);
					_idToContent.Add(kvp.Key, item);
					_content.Add(item);
					AccumulateContentTags(item);

					item.OnEnriched += ContentItemDescriptor_OnEnriched;
					item.OnRenamed += ContentItemDescriptor_OnRenamed;
					OnContentAdded?.Invoke(item);
				}

			}

			RefreshFilteredContents();

		}

		public void SetServerContent(Manifest serverManifest)
		{
			ServerManifest = serverManifest;
			var publicOnly = serverManifest.References.Where(r => r.Visibility == "public").ToList();
			_lastServerManifest = publicOnly.ToDictionary(x => x.Id);
			// enrich the local content
			var unprocessedIds = new HashSet<string>(_idToContent.Keys);
			foreach (var reference in publicOnly)
			{
				unprocessedIds.Remove(reference.Id);

				if (_nameToType.TryGetValue(reference.TypeName, out var existingTypeDesc))
				{
					existingTypeDesc.EnrichWithServer();
				}
				else
				{
					var newTypeDesc = new ContentTypeDescriptor();
					newTypeDesc.SetFromServer(reference);
					AddType(newTypeDesc);
				}

				if (_idToContent.TryGetValue(reference.Id, out var existingContent))
				{
					existingContent.EnrichWithServerData(reference);
					AccumulateContentTags(existingContent);
				}
				else
				{
					var item = new ContentItemDescriptor(reference, _nameToType[reference.TypeName]);
					AccumulateContentTags(item);

					_idToContent.Add(reference.Id, item);

					_content.Add(item);
					OnContentAdded?.Invoke(item);
					item.OnEnriched += ContentItemDescriptor_OnEnriched;

				}
			}

			// mark the remaining local ones correctly
			foreach (var unProcessedId in unprocessedIds)
			{
				_idToContent[unProcessedId].EnrichWithNoServerData();
			}
			RefreshFilteredContents();

		}

		private IList<ContentItemDescriptor> _selectedContent;
		public IList<ContentItemDescriptor> SelectedContents
		{
			set
			{
				_selectedContent = value;

				if (IsDebugging)
				{
					string selectionString = "";
					foreach (var x in _selectedContent)
					{
						selectionString += x.Name + ",";
					}
					BeamableLogger.Log("CDM.SelectedContents() :" + selectionString);
				}

				OnSelectedContentChanged?.Invoke(_selectedContent);
			}
			get
			{
				_selectedContent = _selectedContent ?? new List<ContentItemDescriptor>();

				return _selectedContent;
			}
		}

		public void ClearSelectedContents()
		{
			SelectedContents = new List<ContentItemDescriptor>();
		}

		public IEnumerable<ContentItemDescriptor> GetAllContents()
		{
			return _content.ToList();
			//         foreach (var content in _content)
			//         {
			//            yield return content;
			//         }
		}

		public IEnumerable<ContentItemDescriptor> GetFilteredContents()
		{
			foreach (var content in _content)
			{
				if (Filter != null && !Filter.Accepts(content)) continue;
				if (SystemFilter != null && !SystemFilter.Accepts(content)) continue;

				yield return content;
			}
		}

		private List<ContentItemDescriptor> _filteredContents = new List<ContentItemDescriptor>();
		public List<ContentItemDescriptor> FilteredContents => _filteredContent;

		public void RefreshFilteredContents()
		{
			_filteredContent.Clear();
			for (int i = 0; i < _content.Count; i++)
			{
				if (Filter != null && !Filter.Accepts(_content[i])) continue;
				if (SystemFilter != null && !SystemFilter.Accepts(_content[i])) continue;

				_filteredContent.Add(_content[i]);
			}

			_filteredContent.Sort(CurrentSorter);
			OnFilteredContentsChanged?.Invoke();
		}

		public IEnumerable<ContentTagDescriptor> GetAllTagDescriptors()
		{
			foreach (var kvp in _tagToDescriptor)
			{
				yield return kvp.Value;
			}
		}

		public IEnumerable<string> GetAllTags()
		{
			foreach (var kvp in _tagToDescriptor)
			{
				yield return kvp.Key;
			}
		}

		public List<ContentTypeTreeViewItem> ContentTypeTreeViewItems()
		{
			var types = _nameToType.Values.ToList();
			types.Sort((a, b) => String.Compare(a.TypeName, b.TypeName, StringComparison.Ordinal));

			var output = new List<ContentTypeTreeViewItem>();
			var depth = 0;
			var nameStack = new Stack<string>();
			var depthStack = new Stack<int>();

			for (var i = 0; i < types.Count; i++)
			{
				var type = types[i];
				var j = i - 1;

				if (j >= 0)
				{
					var lastType = types[j];
					/*
					 * a
					 * a.b // does "a.b" start with "a."
					 * TODO write unit tests for this.
					 */
					if (type.TypeName.StartsWith(lastType.TypeName + "."))
					{
						// this type is a child of the previous type, store this moment in memory.
						depthStack.Push(depth);
						nameStack.Push(lastType.TypeName);

						depth += 1;
					}
					else
					{
						// need to move back to the last matching time.
						while (nameStack.Count > 0 && !type.TypeName.StartsWith(nameStack.Peek()))
						{
							depth = depthStack.Pop();
							nameStack.Pop();
						}
					}
				}

				// Start with id=1 here, as root node has id=0
				int id = i + 1;
				output.Add(new ContentTypeTreeViewItem(id, depth, type));
			}

			return output;
		}


		private IList<TreeViewItem> _selectedContentTypes = new List<TreeViewItem>();
		public IList<TreeViewItem> SelectedContentTypes
		{
			set
			{
				_selectedContentTypes = value;

				if (IsDebugging)
				{
					string selectionString = "";
					foreach (var x in _selectedContentTypes)
					{
						selectionString += x.displayName + ",";
					}
					BeamableLogger.Log("CDM.SelectedContentTypes() :" + selectionString);
				}

				var viewItems = _selectedContentTypes.Cast<ContentTypeTreeViewItem>().ToList();

				// TODO. Support multiple type queries in one filter.
				var selection = viewItems.FirstOrDefault()?.TypeDescriptor?.ContentType;
				SystemFilter.TypeConstraints = selection == null
				   ? new HashSet<Type>()
				   : new HashSet<Type> { selection };

				RefreshFilteredContents();
				OnFilterChanged?.Invoke();

				OnSelectedContentTypesChanged?.Invoke(_selectedContentTypes);

				//  ******************************************************
				//  NOTE: Every time the SelectedContentTypes is SET,
				//        the SelectedContents is CLEARED
				//  ******************************************************
				ClearSelectedContents();
			}
			get
			{
				return _selectedContentTypes;
			}
		}

		private IList<TreeViewItem> _selectedContentTypeBranch = new List<TreeViewItem>();
		public Manifest ServerManifest { get; private set; }
		public List<ContentTypePair> ContentTypePairs { get; private set; }
		public LocalContentManifest LocalManifest { get; private set; }
		public IList<TreeViewItem> SelectedContentTypeBranch
		{
			set
			{
				_selectedContentTypeBranch = value;

				if (IsDebugging)
				{
					string selectionString = "";
					foreach (var x in _selectedContentTypeBranch)
					{
						selectionString += x.displayName + ",";
					}
					BeamableLogger.Log("CDM.SelectedContentTypeBranch() :" + selectionString);
				}

				OnSelectedContentTypeBranchChanged?.Invoke(_selectedContentTypeBranch);
			}
			get
			{
				return _selectedContentTypeBranch;
			}
		}

		public void ClearSelectedContentTypes()
		{
			SelectedContentTypes = new List<TreeViewItem>();
		}


		public IEnumerable<ContentTypeDescriptor> GetContentTypes()
		{
			foreach (var kvp in _nameToType)
			{
				yield return kvp.Value;
			}
		}

		public void SetFilter(string query)
		{
			SetFilter(EditorContentQuery.Parse(query));
		}
		public void SetFilter(EditorContentQuery query)
		{
			var changed = !query.ToString().Equals(Filter?.ToString());

			// if we are adding a type to the query, and there wasn't one before... then jump the selection back to top-level
			var oldFilterHadNoType = (Filter?.TypeConstraints == null || Filter?.TypeConstraints.Count == 0);
			var nextFilterHasType = query?.TypeConstraints != null && query?.TypeConstraints.Count > 0;
			if (oldFilterHadNoType && nextFilterHasType)
			{
				SystemFilter.TypeConstraints = null;
				ClearSelectedContentTypes();
			}

			Filter = EditorContentQuery.Parse(query.ToString());

			if (changed)
			{
				RefreshFilteredContents();
				OnQueryUpdated?.Invoke(Filter, false);
				OnFilterChanged?.Invoke();
			}

		}
		public void ContentItemRename(ContentItemDescriptor contentItemDescriptor)
		{
			if (_content.IndexOf(contentItemDescriptor) == -1)
			{
				BeamableLogger.LogError(new Exception("ContentItemDescriptor_OnModified() error. Item must exist in list."));
				return;
			}

			UnityEngine.Object unityObject =
			   AssetDatabase.LoadMainAssetAtPath(contentItemDescriptor.AssetPath);

			ContentObject contentObject = (ContentObject)unityObject;

			if (contentObject == null)
			{
				BeamableLogger.LogError(new Exception("ContentItemDescriptor_OnModified() error. contentObject must be not null."));
				return;
			}

			var oldAssetPath = contentItemDescriptor.AssetPath;
			var newPath = Path.Combine(Path.GetDirectoryName(oldAssetPath), $"{contentItemDescriptor.Name}.asset");

			ContentIO.Rename(oldAssetPath, newPath, contentObject);
			//         _contentIO.RenameByType(contentItemDescriptor.ContentType.ContentType,
			//            contentObject, contentItemDescriptor.Name);
		}

		public void HandleContentRenamed(string oldId, IContentObject content, string nextAssetPath)
		{
			if (_idToContent.TryGetValue(oldId, out var oldItem))
			{
				var typeName = ContentTypeReflectionCache.GetTypeNameFromId(content.Id);
				if (!_nameToType.ContainsKey(typeName))
				{
					var newTypeDesc = new ContentTypeDescriptor();
					newTypeDesc.SetFromContent(content);
					_nameToType.Add(typeName, newTypeDesc);
				}

				ContentTypeDescriptor contentTypeDescriptor = _nameToType[typeName];
				if (_idToContent.TryGetValue(content.Id, out var existingItem))
				{
					// if the content is not local, then we can assume its identity...
					if (existingItem.LocalStatus == HostStatus.NOT_AVAILABLE && oldItem.ServerStatus == HostStatus.NOT_AVAILABLE)
					{
						oldItem.EnrichWithLocalData(content, nextAssetPath);
						oldItem.EnrichWithServerData(existingItem.GetServerData());
						AccumulateContentTags(oldItem);
						_idToContent[content.Id] = oldItem;
						_content.Remove(existingItem);
						_idToContent.Remove(oldId);
						OnContentDeleted?.Invoke(existingItem);

					}
					else if (existingItem.LocalStatus == HostStatus.NOT_AVAILABLE &&
							 oldItem.ServerStatus == HostStatus.AVAILABLE)
					{
						var oldServerData = oldItem.GetServerData();
						var existingServerData = existingItem.GetServerData();
						var oldLocal = oldItem.GetLocalContent();
						oldItem.EnrichWithLocalData(content, nextAssetPath);
						oldItem.EnrichWithServerData(existingServerData);

						existingItem.EnrichWithNoLocalData();
						existingItem.EnrichWithServerData(oldServerData);

						AccumulateContentTags(oldItem);
						AccumulateContentTags(existingItem);

						_idToContent[oldId] = existingItem;
						_idToContent[content.Id] = oldItem;

					}
					else
					{
						throw new Exception("name is already taken");
					}
				}
				else
				{
					// The new name is new!

					if (oldItem.ServerStatus != HostStatus.AVAILABLE)
					{
						oldItem.EnrichWithLocalData(content, nextAssetPath);
						AccumulateContentTags(oldItem);

						if (_lastServerManifest != null && _lastServerManifest.TryGetValue(content.Id, out var manifestEntry))
						{
							oldItem.EnrichWithServerData(manifestEntry);
						}

						_idToContent.Remove(oldId);
						_idToContent.Add(content.Id, oldItem);
					}
					else
					{
						oldItem.EnrichWithLocalData(content, nextAssetPath);

						var oldServerContent = oldItem.GetServerData();
						oldItem.EnrichWithNoServerData();
						var item = new ContentItemDescriptor(oldServerContent, oldItem.ContentType);
						AccumulateContentTags(item);

						_idToContent[oldId] = item;
						_idToContent.Add(content.Id, oldItem);

						_content.Add(item);
						OnContentAdded?.Invoke(item);
						item.OnEnriched += ContentItemDescriptor_OnEnriched;
						item.OnRenamed += ContentItemDescriptor_OnRenamed;
					}




				}


				// string assetPath = _contentIO.GetAssetPathByType(contentTypeDescriptor.ContentType, content);


				//_content.Add(item);
				//OnContentAdded?.Invoke(item);
				//oldItem.OnEnriched += ContentItemDescriptor_OnEnriched;
			}
			RefreshFilteredContents();

		}

		public void HandleContentsDeleted(List<ContentDatabaseEntry> contents)
		{
			foreach (var entry in contents)
			{
				if (!_idToContent.TryGetValue(entry.contentId, out var existing))
				{
					continue;
				}

				if (existing.ServerStatus == HostStatus.AVAILABLE)
				{
					// don't delete the whole record, just remove the local aspect...
					existing.EnrichWithNoLocalData();
				}
				else
				{
					_content.Remove(existing);
					_idToContent.Remove(entry.contentId);
				}
			}

			RebuildTagSet();

			foreach (var entry in contents)
			{
				if (!_idToContent.TryGetValue(entry.contentId, out var existing))
				{
					continue;
				}

				if (existing.ServerStatus != HostStatus.AVAILABLE)
				{
					OnContentDeleted?.Invoke(existing);
				}
			}

			RefreshFilteredContents();

		}

		[Obsolete("Do not use.")]
		public void HandleContentDeleted(ContentDatabaseEntry content)
		{
			if (_idToContent.TryGetValue(content.contentId, out var existing))
			{
				/* DELETING MEANS DIFFERENT THINGS BASED ON STATUS.
					if the content is only local, then we can actually get rid of it.
					if the content had server-status, then we don't want to trash it.
				 */
				if (existing.ServerStatus == HostStatus.AVAILABLE)
				{
					// don't delete the whole record, just remove the local aspect...
					existing.EnrichWithNoLocalData();
					RebuildTagSet();
				}
				else
				{
					// the content wasn't available on the server anyway, so we can completely remove it from this list
					_content.Remove(existing);
					_idToContent.Remove(content.contentId);
					RebuildTagSet();
					OnContentDeleted?.Invoke(existing);
				}
			}
			RefreshFilteredContents();

		}

		public void HandleContentAdded(List<IContentObject> contents)
		{
			foreach (var content in contents)
			{
				if (_idToContent.TryGetValue(content.Id, out var existing))
				{
					// it already exists, don't do anything?
				}
				else
				{
					var typeName = ContentTypeReflectionCache.GetTypeNameFromId(content.Id);
					if (!_nameToType.ContainsKey(typeName))
					{
						var newTypeDesc = new ContentTypeDescriptor();
						newTypeDesc.SetFromContent(content);
						_nameToType.Add(typeName, newTypeDesc);
					}

					ContentTypeDescriptor contentTypeDescriptor = _nameToType[typeName];
					string assetPath = ContentIO.GetAssetPathByType(contentTypeDescriptor.ContentType, content);
					var item = new ContentItemDescriptor(content, contentTypeDescriptor, assetPath);
					AccumulateContentTags(item);

					if (_lastServerManifest != null &&
						_lastServerManifest.TryGetValue(content.Id, out var manifestEntry))
					{
						item.EnrichWithServerData(manifestEntry);
					}

					_idToContent.Add(content.Id, item);
					_content.Add(item);
					OnContentAdded?.Invoke(item);
					item.OnEnriched += ContentItemDescriptor_OnEnriched;
					item.OnRenamed += ContentItemDescriptor_OnRenamed;
				}
			}

			RefreshFilteredContents();
		}

		private void ContentItemDescriptor_OnEnriched(ContentItemDescriptor contentItemDescriptor)
		{
			AccumulateContentTags(contentItemDescriptor);
			OnItemEnriched?.Invoke(contentItemDescriptor);
		}

		private void ContentItemDescriptor_OnRenamed(ContentItemDescriptor contentItemDescriptor)
		{
			ContentItemRename(contentItemDescriptor);
		}

		private void AddType(ContentTypeDescriptor type)
		{
			_nameToType.Add(type.TypeName, type);
			OnTypeAdded?.Invoke(type);
		}

		public int CountModificationStatus(ContentModificationStatus status)
		{
			return GetAllContents().Count(c => c.Status == (c.Status & status));
		}

		public int CountValidationStatus(ContentValidationStatus status)
		{
			return GetAllContents().Count(c => c.ValidationStatus == status);
		}

		public ContentCounts GetCounts()
		{
			var invalid = 0;
			var valid = 0;
			var modified = 0;
			var created = 0;
			var deleted = 0;
			var synced = 0;
			foreach (var content in GetAllContents())
			{
				var contentStatus = content.Status;

				valid += content.ValidationStatus == ContentValidationStatus.VALID ? 1 : 0;
				invalid += content.ValidationStatus == ContentValidationStatus.INVALID ? 1 : 0;
				modified += contentStatus == (ContentModificationStatus.MODIFIED & contentStatus) ? 1 : 0;
				created += contentStatus == (ContentModificationStatus.LOCAL_ONLY & contentStatus) ? 1 : 0;
				deleted += contentStatus == (ContentModificationStatus.SERVER_ONLY & contentStatus) ? 1 : 0;
				synced += contentStatus == (ContentModificationStatus.NOT_MODIFIED & contentStatus) ? 1 : 0;
			}

			return new ContentCounts
			{
				Invalid = invalid,
				Valid = valid,
				Modified = modified,
				Created = created,
				Deleted = deleted,
				Synced = synced
			};
		}

		public void RebuildTagSet()
		{
			// we need to totally rebuild the list at this point.
			_tagToDescriptor.Clear();
			_content.ForEach(AccumulateContentTags);
		}

		private void AccumulateContentTags(ContentItemDescriptor item)
		{
			foreach (var tagDescriptor in item.GetAllTags())
			{
				if (_tagToDescriptor.TryGetValue(tagDescriptor.Tag, out var existing))
				{
					// always favor the latest data.
					_tagToDescriptor[tagDescriptor.Tag] = tagDescriptor;
				}
				else
				{
					_tagToDescriptor.Add(tagDescriptor.Tag, tagDescriptor);
				}
			}
		}

		public void ToggleValidationFilter(ContentValidationStatus status, bool shouldFilterOn)
		{
			var statusExistsInFilter = (Filter?.HasValidationConstraint ?? false) && Filter.ValidationConstraint == status;
			var next = new EditorContentQuery(Filter);

			if (statusExistsInFilter && !shouldFilterOn)
			{
				next.HasValidationConstraint = false;
				SetFilter(next);
				OnQueryUpdated?.Invoke(Filter, true);
			}
			else if (!statusExistsInFilter && shouldFilterOn)
			{
				next.HasValidationConstraint = true;
				next.ValidationConstraint = status;
				SetFilter(next);
			}
		}

		public void ToggleStatusFilter(ContentModificationStatus status, bool shouldFilterOn)
		{
			var statusExistsInFilter =
			   (Filter?.HasStatusConstraint ?? false) && (status == (status & Filter.StatusConstraint));

			var next = new EditorContentQuery(Filter);
			if (statusExistsInFilter && !shouldFilterOn)
			{
				next.StatusConstraint = next.StatusConstraint & ~status;
				next.HasStatusConstraint = next.StatusConstraint > 0;
				SetFilter(next);
				OnQueryUpdated?.Invoke(Filter, true);
			}
			else if (!statusExistsInFilter && shouldFilterOn)
			{
				next.StatusConstraint |= status;
				next.HasStatusConstraint = true;
				SetFilter(next);
			}

		}

		public void ToggleTypeFilter(ContentTypeDescriptor type, bool shouldFilterOn)
		{
			var typeExistsInFilter = Filter?.TypeConstraints?.Contains(type.ContentType) ?? false;

			var next = new EditorContentQuery(Filter);
			if (typeExistsInFilter && !shouldFilterOn)
			{
				if (next.TypeConstraints.Count == 1)
				{
					next.TypeConstraints = null;
				}
				else
				{
					next.TypeConstraints.Remove(type.ContentType);
				}
				SetFilter(next);
				OnQueryUpdated?.Invoke(Filter, true);
			}
			else if (!typeExistsInFilter && shouldFilterOn)
			{
				var typeConstraints = next.TypeConstraints ?? new HashSet<Type>();
				typeConstraints.Add(type.ContentType);

				next.TypeConstraints = typeConstraints;
				SetFilter(next);
			}
		}

		public void ToggleTagFilter(string tag, bool shouldFilterOn)
		{
			var tagExistsInFilter = Filter?.TagConstraints?.Contains(tag) ?? false;
			var tags = new HashSet<string>(Filter?.TagConstraints?.ToArray() ?? new string[] { });

			var next = new EditorContentQuery(Filter);

			if (tagExistsInFilter && !shouldFilterOn)
			{
				if (next.TagConstraints.Count == 1)
				{
					next.TagConstraints = null;
				}
				else
				{
					next.TagConstraints.Remove(tag);
				}
				SetFilter(next);
				OnQueryUpdated?.Invoke(Filter, true);

			}
			else if (!tagExistsInFilter && shouldFilterOn)
			{
				var tagConstraints = next.TagConstraints ?? new HashSet<string>();
				tagConstraints.Add(tag);

				next.TagConstraints = tagConstraints;
				SetFilter(next);
			}
		}

		public bool GetDescriptorForId(string contentId, out ContentItemDescriptor descriptor)
		{
			return _idToContent.TryGetValue(contentId, out descriptor);
		}

		public bool GetLatestDescriptor(ContentItemDescriptor descriptor, out ContentItemDescriptor latest)
		{
			return _idToContent.TryGetValue(descriptor.Id, out latest);
		}

		public void SetSorter(ContentSortType newSorter)
		{
			if (CurrentSorter == newSorter)
				return;

			CurrentSorter = newSorter;
			RefreshFilteredContents();
		}
	}
}
