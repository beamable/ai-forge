using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Editor.Content.SaveRequest;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using static Beamable.Common.Constants.Directories;

namespace Beamable.Editor.Content.Models
{
	public class ContentTagDescriptor
	{
		public readonly string Tag;
		public HostStatus LocalStatus { get; private set; }
		public HostStatus ServerStatus { get; private set; }

		public ContentTagDescriptor(string tag, HostStatus local, HostStatus server)
		{
			Tag = tag;
			LocalStatus = local;
			ServerStatus = server;
		}

		public override int GetHashCode()
		{
			return Tag?.GetHashCode() ?? 1;
		}
	}

	public enum ContentValidationStatus
	{
		VALID,
		INVALID
	}

	[Flags]
	public enum ContentModificationStatus
	{
		LOCAL_ONLY = 1,
		SERVER_ONLY = 2,
		MODIFIED = 4,
		NOT_MODIFIED = 8,
		NOT_AVAILABLE_ANYWHERE = 16
	}

	public static class ContentModificationStatusExtensions
	{
		static Dictionary<ContentModificationStatus, string> statusToString = new Dictionary<ContentModificationStatus, string>
	  {
		 {ContentModificationStatus.LOCAL_ONLY, "local"},
		 {ContentModificationStatus.SERVER_ONLY, "server"},
		 {ContentModificationStatus.MODIFIED, "modified"},
		 {ContentModificationStatus.NOT_MODIFIED, "sync"},
		 {ContentModificationStatus.NOT_AVAILABLE_ANYWHERE, "unavailable"},
	  };
		static Dictionary<string, ContentModificationStatus> stringToStatus = new Dictionary<string, ContentModificationStatus>();

		static ContentModificationStatusExtensions()
		{
			foreach (var kvp in statusToString)
			{
				stringToStatus.Add(kvp.Value, kvp.Key);
			}
		}
		public static string Serialize(this ContentModificationStatus self)
		{
			var str = self.ToString();

			str = str.Replace(ContentModificationStatus.NOT_MODIFIED.ToString(), statusToString[ContentModificationStatus.NOT_MODIFIED]);

			foreach (var kvp in stringToStatus)
			{
				str = str.Replace(kvp.Value.ToString(), kvp.Key);
			}

			str = str.Replace(",", "");
			return str;
		}

		public static bool TryParse(string str, out ContentModificationStatus status)
		{
			var parts = str.Split(new[] { ' ' }, StringSplitOptions.None);
			status = ContentModificationStatus.NOT_AVAILABLE_ANYWHERE;

			var any = false;
			foreach (var part in parts)
			{
				if (stringToStatus.TryGetValue(part, out var subStatus))
				{
					if (!any)
					{
						status = subStatus;
					}
					else
					{
						status |= subStatus;
					}
					any = true;
				}
			}

			return any;
		}

	}


	public enum HostStatus
	{
		UNKNOWN,
		AVAILABLE,
		NOT_AVAILABLE
	}

	public class ContentItemDescriptor
	{
		/// <summary>
		/// Invoked when name change is requested.
		/// </summary>
		public event Action<ContentItemDescriptor> OnRenamed;

		/// <summary>
		/// Invoked when the name should be changed by a user
		/// </summary>
		public event Action OnRenameRequested;

		/// <summary>
		/// Invoked when any change is made
		/// </summary>
		public event Action<ContentItemDescriptor> OnEnriched;



		private string _name = "";
		public string Name
		{
			set
			{
				if (string.Equals(_name, value)) return;
				if (string.IsNullOrWhiteSpace(value)) throw new Exception("Name cannot be empty.");
				var oldName = _name;
				try
				{
					_name = value;
					OnRenamed?.Invoke(this);
				}
				catch (Exception)
				{
					_name = oldName; // clean up the name
					throw;
				}
			}
			get
			{
				return _name;
			}
		}
		public ContentTypeDescriptor ContentType { get; private set; }

		public ContentValidationStatus ValidationStatus => _validationExceptions?.Count > 0
		   ? ContentValidationStatus.INVALID
		   : ContentValidationStatus.VALID;
		private List<ContentException> _validationExceptions;

		private string _lastChecksum;
		private Guid _lastValidationGuid;

		public ContentModificationStatus Status
		{
			get
			{
				if (_localContent != null && _serverData == null)
				{
					return ContentModificationStatus.LOCAL_ONLY;
				}

				if (_localContent == null && _serverData != null)
				{
					return ContentModificationStatus.SERVER_ONLY;
				}

				if (_localContent != null && _serverData != null)
				{
					return (ContentIO.ComputeChecksum(_localContent).Equals(_serverData.Checksum) && ContentIO.AreTagsEqual(_localContent.Tags, _serverData.Tags))
					   ? ContentModificationStatus.NOT_MODIFIED
					   : ContentModificationStatus.MODIFIED;
				}

				return ContentModificationStatus.NOT_AVAILABLE_ANYWHERE;
			}
		}

		private string _assetPath = "";
		public string AssetPath
		{
			private set
			{
				_assetPath = value;
			}
			get
			{
				return _assetPath;
			}
		}

		/// <summary>
		/// A display version of the asset path that is relative to the data dir, and removes the asset file extension.
		/// </summary>
		public string AssetPathShort =>
		   string.IsNullOrEmpty(AssetPath)
			  ? ""
			  : Path.GetDirectoryName(AssetPath)?.Substring(DATA_DIR.Length + 1) ?? "";


		public HashSet<string> AllTags => new HashSet<string>(GetAllTags().Select(x => x.Tag)); // TODO: This could be cached.

		public HashSet<string> LocalTags { get; private set; }
		public HashSet<string> ServerTags { get; private set; }

		public HostStatus LocalStatus { get; private set; }
		public HostStatus ServerStatus { get; private set; }
		public string Id => $"{ContentType.TypeName}.{Name}";

		public string GetFormattedLastChanged => LastChanged == 0
			? string.Empty
			: DateTimeOffset.FromUnixTimeMilliseconds(LastChanged).DateTime
							.ToLocalTime()
							.ToString("HH:mm, MM/dd/yyyy", CultureInfo.GetCultureInfo("en-US"));

		public long LastChanged { get; private set; }
		public bool IsCorrupted => ContentException != null;
		public ContentCorruptedException ContentException { get; private set; }

		private LocalContentManifestEntry _localData;
		private IContentObject _localContent;
		private ManifestReferenceSuperset _serverData;
		private List<ContentTagDescriptor> _allTags;
		private string _localChecksum;
		private long _serverLastChanged;

		public ContentItemDescriptor(IContentObject content, ContentTypeDescriptor typeDescriptor, string assetPath)
		{
			_name = content.Id.Split('.').Last();
			AssetPath = assetPath;
			ContentType = typeDescriptor;
			LocalTags = new HashSet<string>(content.Tags);
			ServerStatus = HostStatus.NOT_AVAILABLE;
			LocalStatus = HostStatus.AVAILABLE;
			_serverLastChanged = content.LastChanged;
			LastChanged = _serverLastChanged;
			ContentException = content.ContentException;
			_localContent = content;
			_allTags = CollectAllTags();

			SetupLocalEventListeners(_localContent as ContentObject);
		}

		public ContentItemDescriptor(LocalContentManifestEntry entry, ContentTypeDescriptor typeDescriptor)
		{
			_localData = entry;
			_localContent = entry.Content;
			_assetPath = _localData?.AssetPath ?? "";

			_name = _localData.Id.Split('.').Last();
			ContentType = typeDescriptor;
			LocalTags = new HashSet<string>(_localData.Tags);
			ServerStatus = HostStatus.UNKNOWN;
			LocalStatus = HostStatus.AVAILABLE;
			_serverLastChanged = entry.LastChanged;
			LastChanged = _serverLastChanged;
			ContentException = entry.ContentException;
			_allTags = CollectAllTags();

			SetupLocalEventListeners(_localContent as ContentObject);
		}

		public ContentItemDescriptor(ManifestReferenceSuperset entry, ContentTypeDescriptor typeDescriptor)
		{
			_serverData = entry;
			_name = entry.Id.Split('.').Last();
			ContentType = typeDescriptor;
			ServerTags = new HashSet<string>(entry.Tags);
			ServerStatus = HostStatus.AVAILABLE;
			LocalStatus = HostStatus.NOT_AVAILABLE;
			_serverLastChanged = entry.LastChanged;
			LastChanged = _serverLastChanged;
			_allTags = CollectAllTags();
		}

		public void EnrichWithServerData(ManifestReferenceSuperset reference)
		{
			try
			{
				_name = reference.Id.Split('.').Last();
				_serverData = reference;
				ServerStatus = HostStatus.AVAILABLE;
				ServerTags = new HashSet<string>(reference.Tags);
				_serverLastChanged = reference.LastChanged != 0 ? reference.LastChanged : reference.Created;
				LastChanged = _serverLastChanged;
				_allTags = CollectAllTags();
			}
			catch (Exception ex)
			{
				BeamableLogger.LogException(ex);
			}

			OnEnriched?.Invoke(this);
		}

		public void EnrichWithLocalData(IContentObject content, string assetPath)
		{
			ContentObject contentObject = _localContent as ContentObject;

			RemoveLocalEventListeners(contentObject);
			_localContent = content;
			_name = content.Id.Split('.').Last();
			AssetPath = assetPath;
			LocalTags = new HashSet<string>(content.Tags);
			_allTags = CollectAllTags();
			LocalStatus = HostStatus.AVAILABLE;
			_serverLastChanged = content.LastChanged;
			LastChanged = _serverLastChanged;
			ContentException = content.ContentException;

			SetupLocalEventListeners(contentObject);
		}

		public void EnrichWithLocalData(LocalContentManifestEntry entry)
		{

			_localData = entry;
			_name = _localData.Id.Split('.').Last();
			_localContent = entry.Content;
			SetupLocalEventListeners(_localContent as ContentObject);
			LocalTags = new HashSet<string>(entry.Tags);
			LocalStatus = HostStatus.AVAILABLE;
			AssetPath = entry.AssetPath;
			_allTags = CollectAllTags();
			_serverLastChanged = entry.LastChanged;
			LastChanged = _serverLastChanged;
			ContentException = entry.ContentException;

			OnEnriched?.Invoke(this);
		}

		public void EnrichWithNoServerData()
		{
			ServerStatus = HostStatus.NOT_AVAILABLE;
			_serverData = null;
			_allTags = CollectAllTags();

			OnEnriched?.Invoke(this);
		}

		public void EnrichWithNoLocalData()
		{
			LocalStatus = HostStatus.NOT_AVAILABLE;
			_localContent = null;
			_localData = null;
			AssetPath = null;
			LocalTags = null;
			_allTags = CollectAllTags();
			_serverLastChanged = 0;
			LastChanged = 0;
			ContentException = null;
			OnEnriched?.Invoke(this);
		}

		private void RemoveLocalEventListeners(ContentObject contentObject)
		{
			if (contentObject == null) return;

			contentObject.OnEditorValidation -= ContentObject_OnEditorValidate;
			contentObject.OnValidationChanged -= ContentObject_OnValidationChanged;
		}
		private void SetupLocalEventListeners(ContentObject contentObject)
		{
			if (contentObject == null) return;

			contentObject.OnEditorValidation += ContentObject_OnEditorValidate;
			contentObject.OnValidationChanged += ContentObject_OnValidationChanged;
		}

		private void ContentObject_OnEditorValidate()
		{
			// compute checksum, and if its different, launch an enriched event.
			var contentObj = _localContent as ContentObject;
			var currChecksum = contentObj == null
			   ? ""
			   : ContentIO.ComputeChecksum(contentObj);
			var distinctTagsExist = ContentIO.AreTagsEqual(_localContent.Tags, LocalTags.ToArray());
			if (!string.Equals(_localChecksum, currChecksum) || !distinctTagsExist)
			{
				_allTags = CollectAllTags();
				_localChecksum = currChecksum;
				OnEnriched?.Invoke(this);
			}
		}

		private List<ContentTagDescriptor> CollectAllTags()
		{
			var allTags = new List<ContentTagDescriptor>();
			var unprocTags = new HashSet<string>(ServerTags ?? new HashSet<string>());
			if (_localContent != null)
			{
				LocalTags = new HashSet<string>(_localContent.Tags);
			}
			if (LocalTags != null)
			{
				foreach (var localTag in LocalTags)
				{
					unprocTags.Remove(localTag);
					var serverStatus = HostStatus.UNKNOWN;
					if (ServerStatus == HostStatus.AVAILABLE)
					{
						serverStatus = ServerTags.Contains(localTag) ? HostStatus.AVAILABLE : HostStatus.NOT_AVAILABLE;
					}
					else if (ServerStatus == HostStatus.NOT_AVAILABLE)
					{
						serverStatus = HostStatus.NOT_AVAILABLE;
					}

					allTags.Add(new ContentTagDescriptor(localTag, HostStatus.AVAILABLE, serverStatus));
				}
			}

			foreach (var unproccessedTag in unprocTags)
			{
				allTags.Add(new ContentTagDescriptor(unproccessedTag, HostStatus.NOT_AVAILABLE, HostStatus.AVAILABLE));
			}
			return allTags;
		}

		public IEnumerable<ContentTagDescriptor> GetAllTags()
		{
			foreach (var tag in _allTags)
			{
				yield return tag;
			}
		}

		public IEnumerable<string> GetValidationErrors()
		{
			if (_validationExceptions == null) yield break;
			foreach (var exception in _validationExceptions)
			{
				yield return exception.Message;
			}
		}

		public LocalContentManifestEntry GetLocalContent()
		{
			return _localData;
		}

		public IContentObject GetContent()
		{
			return _localContent;
		}

		public ManifestReferenceSuperset GetServerData()
		{
			return _serverData;
		}

		public void EnrichWithValidationErrors(List<ContentException> exceptions)
		{
			_validationExceptions = exceptions.ToList();
			OnEnriched?.Invoke(this);
		}

		public void EnrichWithNoValidationErrors()
		{
			_validationExceptions = null;
			OnEnriched?.Invoke(this);
		}

		public void RefreshLatestUpdate(bool setOriginalLatestUpdate = false)
		{
			LastChanged = setOriginalLatestUpdate ? _serverLastChanged : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		}

		private void ContentObject_OnValidationChanged(List<ContentException> exceptions)
		{
			if (exceptions == null)
			{
				EnrichWithNoValidationErrors();
			}
			else
			{
				EnrichWithValidationErrors(exceptions);
			}
		}

		public void ForceRename()
		{
			OnRenameRequested?.Invoke();
		}
	}
}
