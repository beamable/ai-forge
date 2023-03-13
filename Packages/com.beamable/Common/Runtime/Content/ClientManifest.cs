using Beamable.Common.Api.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beamable.Common.Content
{
	/// <summary>
	/// This type defines the %ClientManifest for the %ContentService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Content.ContentService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[System.Serializable]
	public class ClientManifest
	{
		/// <summary>
		/// A set of <see cref="ClientContentInfo"/> that exist in this <see cref="ClientManifest"/>.
		/// Each <see cref="ClientContentInfo"/> describes one piece of content.
		/// </summary>
		public List<ClientContentInfo> entries = new List<ClientContentInfo>();

		/// <summary>
		/// Use a <see cref="ContentQuery"/> to filter the <see cref="entries"/> and get a new <see cref="ClientManifest"/>.
		/// This method will not mutate the <i>current</i> <see cref="ClientManifest"/>. Instead, it allocates a new one.
		/// </summary>
		/// <param name="query">A <see cref="ContentQuery"/> to down filter the <see cref="entries"/></param>
		/// <returns>A new <see cref="ClientManifest"/></returns>
		public ClientManifest Filter(ContentQuery query)
		{
			return new ClientManifest { entries = entries.Where(e => query.Accept(e)).ToList() };
		}

		/// <summary>
		/// Use a string version of a <see cref="ContentQuery"/> to filter the <see cref="entries"/> and get a new <see cref="ClientManifest"/>.
		/// This method will not mutate the <i>current</i> <see cref="ClientManifest"/>. Instead, it allocates a new one.
		/// </summary>
		/// <param name="queryString">A string version of a <see cref="ContentQuery"/> to down filter the <see cref="entries"/></param>
		/// <returns>A new <see cref="ClientManifest"/></returns>
		public ClientManifest Filter(string queryString)
		{
			return Filter(ContentQuery.Parse(queryString));
		}

		/// <summary>
		/// The <see cref="entries"/> only describe the content, but don't contain the entire content data.
		/// This method will return every <see cref="ClientContentInfo"/> in the <see cref="entries"/> set.
		/// This may result in many network requests if the <see cref="entries"/> haven't been downloaded before.
		/// </summary>
		/// <param name="batchSize">
		/// The <see cref="batchSize"/> controls how many concurrent network requests will be allowed to run simultaneously.
		/// </param>
		/// <returns>
		/// A <see cref="SequencePromise{T}"/> representing the progress of the content resolution. At the end, it will have a
		/// list of <see cref="IContentObject"/> for each <see cref="ClientContentInfo"/> in the <see cref="entries"/> set
		/// </returns>
		public SequencePromise<IContentObject> ResolveAll(int batchSize = 50)
		{
			return entries.ResolveAll(batchSize);
		}

		/// <summary>
		/// The <see cref="ClientManifest"/> is represented as a CSV when delivered to the game client.
		/// This method is used internally to parse the CSV.
		/// </summary>
		/// <param name="data">Raw CSV data</param>
		/// <returns>A <see cref="ClientManifest"/></returns>
		public static ClientManifest ParseCSV(string data) => new CsvManifestScanner(data).Parse();
	}

	/// <summary>
	/// This type defines the %ClientContentInfo for the %ContentService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Content.ContentService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[System.Serializable]
	public class ClientContentInfo
	{
		/// <summary>
		/// The full content id. A content id is a dot separated string with at least one dot.
		/// The right-most clause is the name of the content, and everything else represents the type of the content.
		/// </summary>
		public string contentId;

		/// <summary>
		/// A checksum of the content's data.
		/// </summary>
		public string version;

		/// <summary>
		/// The public uri where the full content json can be downloaded
		/// </summary>
		public string uri;

		/// <summary>
		/// An internal field that will be removed in a future version.
		/// This is <b>NOT</b> the C# backing type of the content.
		/// </summary>
		public string type;

		/// <summary>
		/// The id of the manifest that this content was sourced from.
		/// For most cases, this will be the default manifest, "global".
		/// </summary>
		public string manifestID;

		/// <summary>
		/// An internal field that will be removed in a future version.
		/// This will always be <see cref="ContentVisibility.Public"/>
		/// </summary>
		public ContentVisibility visibility = ContentVisibility.Public;

		/// <summary>
		/// A set of content tags. Tags do not effect the <see cref="version"/> checksum.
		/// </summary>
		public string[] tags;

		/// <summary>
		/// Convert this <see cref="ClientContentInfo"/> into a <see cref="IContentRef"/> by using the <see cref="contentId"/> field.
		/// This method verifies that the backing C# class exists.
		/// </summary>
		/// <returns>A <see cref="IContentRef{TContent}"/></returns>
		public IContentRef ToContentRef()
		{
			var contentType = ContentTypeReflectionCache.Instance.GetTypeFromId(contentId);
			return new ContentRef(contentType, contentId);
		}

		/// <summary>
		/// This object only describes the content, but does not contain the entire content data.
		/// This method will get the actual <see cref="IContentObject"/> by checking for the data at the <see cref="uri"/>.
		/// This may result in a network request if the entry has not been downloaded before.
		/// </summary>
		/// <returns></returns>
		public Promise<IContentObject> Resolve()
		{
			return ContentApi.Instance.FlatMap(api => api.GetContent(ToContentRef()));
		}
	}

	/// <summary>
	/// This type defines the %ClientContentInfoExtensions for the %ContentService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Content.ContentService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public static class ClientContentInfoExtensions
	{
		public static IEnumerable<IContentRef> ToContentRefs(this IEnumerable<ClientContentInfo> set)
		{
			return set.Select(info => info.ToContentRef());
		}

		public static SequencePromise<IContentObject> ResolveAll(this IEnumerable<ClientContentInfo> set,
																 int batchSize = 50)
		{
			return set.ToContentRefs().ResolveAll(batchSize);
		}
	}

	public class CsvManifestScanner
	{
		private readonly string _source;
		private readonly int _sourceLength;
		List<ClientContentInfo> _contentEntries;
		private int _start = 0, _current = 0;
		private int _currentStep = 0, _currentEntry = 0;

		public CsvManifestScanner(string source)
		{
			_sourceLength = string.IsNullOrWhiteSpace(source) ? 0 : source.Length;
			_source = _sourceLength == 0 ? string.Empty : source;
		}

		public ClientManifest Parse()
		{
			_contentEntries = new List<ClientContentInfo>();
			_start = _current = _currentEntry = _currentStep = 0;
			while (!IsAtEnd())
			{
				_start = _current;
				ScanToken();
			}

			return new ClientManifest { entries = _contentEntries };
		}

		private void ScanToken()
		{
			var c = Advance();
			switch (c)
			{
				case ',':
					_currentStep++;
					break;
				case '\n':
					_currentEntry++;
					_currentStep = 0;
					break;
				case '"':
					while (Peek() != '"' && !IsAtEnd())
					{
						Advance();
					}

					if (IsAtEnd())
					{
						Debug.LogError("Unterminated value in double quote");
						return;
					}

					Advance();
					AddInfo(_start + 1, _current - _start - 2);
					break;
				default:
					while (Peek() != ',' && Peek() != '\n')
					{
						Advance();
					}

					AddInfo(_start, _current - _start);
					break;
			}
		}

		private void AddInfo(int startIndex, int length)
		{
			if (_currentEntry >= _contentEntries.Count)
			{
				_contentEntries.Add(new ClientContentInfo
				{
					visibility = ContentVisibility.Public,
					tags = new string[] { }
				});
			}

			var entryValue = _source.Substring(startIndex, length).Trim();

			const int STEP_TYPE = 0;
			const int STEP_CONTENT_ID = 1;
			const int STEP_VERSION = 2;
			const int STEP_URI = 3;
			const int STEP_TAGS = 4;

			switch (_currentStep)
			{
				case STEP_TYPE:
					_contentEntries[_currentEntry].type = entryValue;
					break;
				case STEP_CONTENT_ID:
					_contentEntries[_currentEntry].contentId = entryValue;
					break;
				case STEP_VERSION:
					_contentEntries[_currentEntry].version = entryValue;
					break;
				case STEP_URI:
					_contentEntries[_currentEntry].uri = entryValue;
					break;
				case STEP_TAGS:
					if (!string.IsNullOrWhiteSpace(entryValue))
					{
						_contentEntries[_currentEntry].tags =
							entryValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					}

					break;
				default:
					Debug.LogError("Value out of range");
					break;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		char Advance() => _source[_current++];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		char Peek() => IsAtEnd() ? '\n' : _source[_current];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool IsAtEnd() => _current >= _sourceLength;
	}
}
