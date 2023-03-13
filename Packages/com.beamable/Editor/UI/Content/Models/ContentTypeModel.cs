using Beamable.Common.Content;
using Beamable.Editor.Content.SaveRequest;
using System;
using UnityEditor.IMGUI.Controls;

namespace Beamable.Editor.Content.Models
{
	public class ContentTypeTreeViewItem : TreeViewItem
	{
		public ContentTypeDescriptor TypeDescriptor { get; }

		public ContentTypeTreeViewItem(int id, int depth, ContentTypeDescriptor typeDescriptor)
		{
			TypeDescriptor = typeDescriptor;

			base.id = id;
			base.displayName = typeDescriptor.ShortName;
			base.depth = depth;
		}
	}


	public class ContentTypeDescriptor
	{
		public Type ContentType { get; private set; }
		public string TypeName { get; private set; }

		public string ShortName
		{
			get
			{
				var lastIndex = TypeName.LastIndexOf('.');
				return lastIndex < 0
					   ? TypeName
					   : TypeName.Substring(lastIndex + 1);
			}
		}

		public HostStatus LocalStatus { get; private set; }
		public HostStatus ServerStatus { get; private set; }

		public Action OnModified;

		// TODO refactor to constructor
		public void SetFromLocal(string typeName, Type type)
		{
			ContentType = type;
			TypeName = typeName;
			LocalStatus = HostStatus.AVAILABLE;
			ServerStatus = HostStatus.UNKNOWN;
		}

		public void SetFromLocal(LocalContentManifestEntry entry)
		{
			ContentType = entry.ContentType;
			TypeName = entry.TypeName;
			LocalStatus = HostStatus.AVAILABLE;
			ServerStatus = HostStatus.UNKNOWN;
		}

		public void SetFromContent(IContentObject content)
		{
			var contentTypeReflectionCache = BeamEditor.GetReflectionSystem<ContentTypeReflectionCache>();

			ContentType = content.GetType();
			TypeName = contentTypeReflectionCache.TypeToName(ContentType);
			LocalStatus = HostStatus.AVAILABLE;
			ServerStatus = HostStatus.UNKNOWN;
		}

		public void SetFromServer(ManifestReferenceSuperset reference)
		{
			TypeName = reference.TypeName;
			ServerStatus = HostStatus.AVAILABLE;

			var contentTypeReflectionCache = BeamEditor.GetReflectionSystem<ContentTypeReflectionCache>();
			if (contentTypeReflectionCache.TryGetType(TypeName, out var type))
			{
				ContentType = type;
				LocalStatus = HostStatus.AVAILABLE;
			}
			else
			{
				LocalStatus = HostStatus.NOT_AVAILABLE;
			}

		}

		public void EnrichWithLocal(Type contentType)
		{
			LocalStatus = HostStatus.AVAILABLE;
			ContentType = contentType;
			OnModified?.Invoke();
		}

		public void EnrichWithServer()
		{
			ServerStatus = HostStatus.AVAILABLE;
			OnModified?.Invoke();
		}
	}
}
