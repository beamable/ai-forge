using Beamable.Common.Content;
using Beamable.Editor.Content.SaveRequest;
using Beamable.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Editor.Content
{

	/// <summary>
	/// This type defines a %Beamable %Manifest containing various metadata in one coherent unit.
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Editor.Content.ContentManifest script reference
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	public class Manifest
	{
		private Dictionary<string, ContentManifestReference> _lookup = new Dictionary<string, ContentManifestReference>();
		private ContentManifest _source;
		private IEnumerable<ContentManifestReference> _allReferences;
		private string _checksum;

		public long Created => _source?.created ?? 0;

		public List<ManifestReferenceSuperset> References => _allReferences
		   .Select(r => new ManifestReferenceSuperset()
		   {
			   Checksum = r.checksum,
			   Id = r.id,
			   Type = r.type,
			   Uri = r.uri,
			   Visibility = r.visibility,
			   Version = r.version,
			   Tags = r.tags,
			   Created = r.created,
			   LastChanged = r.lastChanged
		   })
		   .ToList(); // shallow copy.

		public Manifest(IEnumerable<ContentManifestReference> references)
		{
			_lookup = references.ToDictionary(r => r.id);
			_allReferences = references;
		}

		public Manifest(ContentManifest source)
		{
			_source = source;
			_allReferences = source.references;
			_checksum = source.checksum;
			_lookup = source.references
			   .Where(r => r.visibility.Equals("public"))
			   .ToDictionary(r => r.id);
		}

		public ContentManifestReference Get(string id)
		{
			_lookup.TryGetValue(id, out ContentManifestReference result);
			return result;
		}

		public static ManifestDifference FindDifferences(Manifest current, Manifest next)
		{
			// a change set between manifests includes MODIFICATIONS, ADDITIONS, and DELETIONS

			var currentIds = current._lookup.Keys;

			var unseenIds = new HashSet<string>();
			next._lookup.Keys.ToList().ForEach(id => unseenIds.Add(id));

			var additions = new List<ContentManifestReference>();
			var modifications = new List<ContentManifestReference>();

			foreach (var id in currentIds)
			{
				// to facilitate deletions, take note of each id we've seen, so that at the end of iteration, the set only contains ids not existing in currentIds
				unseenIds.Remove(id);

				var nextContent = next.Get(id);
				var currentContent = current.Get(id);

				if (nextContent == null)
				{
					// only exists in current. counts as an addition to the next set.
					additions.Add(currentContent);
					continue;
				}
				var distinctTagsExist = ContentIO.AreTagsEqual(nextContent.tags, currentContent.tags);
				if (!nextContent.checksum.Equals(currentContent.checksum) || !distinctTagsExist)
				{
					modifications.Add(currentContent);
				}
			}

			var deletions = unseenIds.Select(id => next.Get(id)).ToList();

			return new ManifestDifference()
			{
				Additions = additions,
				Modifications = modifications,
				Deletions = deletions
			};
		}
	}

	/// <summary>
	/// This type defines a %Beamable %Manifest %Difference
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See Beamable.Editor.Content.ContentManifest script reference
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	public class ManifestDifference
	{
		public IEnumerable<ContentManifestReference> Additions, Deletions, Modifications;
	}

	/// <summary>
	/// This type defines a %Beamable %Manifest %Difference
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-feature">Content</a> feature documentation
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	[System.Serializable]
	public class ContentManifest : JsonSerializable.ISerializable
	{
		public string id;
		public long created;
		public List<ContentManifestReference> references;
		public string checksum;

		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			s.Serialize(nameof(id), ref id);
			s.Serialize(nameof(created), ref created);
			s.SerializeList(nameof(references), ref references);
		}
	}

	/// <summary>
	/// This type defines a %Beamable %Manifest %Reference
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See Beamable.Editor.Content.ContentManifest script reference
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	[System.Serializable]
	public class ContentManifestReference : JsonSerializable.ISerializable
	{
		public string id;
		public string version;
		public string type;
		public string[] tags;
		public string uri;
		public string checksum;
		public string visibility;
		public long created;
		public long lastChanged;

		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			s.Serialize(nameof(id), ref id);
			s.Serialize(nameof(version), ref version);
			s.Serialize(nameof(type), ref type);
			s.SerializeArray(nameof(tags), ref tags);
			s.Serialize(nameof(uri), ref uri);
			s.Serialize(nameof(checksum), ref checksum);
			s.Serialize(nameof(visibility), ref visibility);
			s.Serialize(nameof(created), ref created);
			s.Serialize(nameof(lastChanged), ref lastChanged);
		}
	}

	/// <summary>
	/// This type defines a %Beamable %Local %Content %Manifest
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See Beamable.Editor.Content.ContentManifest script reference
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	[System.Serializable]
	public class LocalContentManifest
	{
		public Dictionary<string, LocalContentManifestEntry> Content = new Dictionary<string, LocalContentManifestEntry>();
	}

	/// <summary>
	/// This type defines a %Beamable %Local %Content %Manifest %Entry
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See Beamable.Editor.Content.ContentManifest script reference
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	[System.Serializable]
	public class LocalContentManifestEntry
	{
		public Type ContentType;
		public string Id => Content.Id;
		public string TypeName => Content.Id.Substring(0, Content.Id.LastIndexOf('.'));
		public string[] Tags => Content.Tags;
		public string Version => Content.Version;
		public string AssetPath;
		public long LastChanged => Content.LastChanged;
		public ContentCorruptedException ContentException => Content.ContentException;
		public IContentObject Content;

	}
}
