using Beamable.Common.Api.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Common.Content
{
	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface IContentLink
	{
		string GetId();
		void SetId(string id);
		void OnCreated();
	}

	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[System.Serializable]
	public abstract class AbsContentLink<TContent> : AbsContentRef<TContent>, IContentLink where TContent : IContentObject, new()
	{
		public abstract void OnCreated(); // the resolution of this method is different based on client/server...
	}

	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[System.Serializable]
	public abstract class BaseContentRef : IContentRef
	{
		public abstract string GetId();

		public abstract void SetId(string id);

		public abstract bool IsContent(IContentObject content);

		public abstract Type GetReferencedType();

		public abstract Type GetReferencedBaseType();

	}

	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	/// <typeparam name="TContent"></typeparam>
	[System.Serializable]
	public abstract class AbsContentRef<TContent> : BaseContentRef, IContentRef<TContent> where TContent : IContentObject, new()
	{
		public string Id;

		public abstract Promise<TContent> Resolve(string manifestID = ""); // the resolution of this method is different based on client/server.

		public override string GetId()
		{
			return Id;
		}

		public override void SetId(string id)
		{
			Id = id;
		}

		public override bool IsContent(IContentObject content)
		{
			return content.Id.Equals(Id);
		}

		public override Type GetReferencedType()
		{
			if (string.IsNullOrEmpty(Id))
				return typeof(TContent);
			return ContentTypeReflectionCache.Instance.GetTypeFromId(Id);
		}

		public override Type GetReferencedBaseType()
		{
			return typeof(TContent);
		}
	}

	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface IContentRef
	{
		string GetId();
		void SetId(string id);
		bool IsContent(IContentObject content);
		Type GetReferencedType();
		Type GetReferencedBaseType();
	}

	public interface IContentRef<TContent> : IContentRef where TContent : IContentObject, new()
	{
		Promise<TContent> Resolve(string manifestID = "");
	}

	public static class IContentRefExtensions
	{
		public static SequencePromise<IContentObject> ResolveAll(this IEnumerable<IContentRef> refs, int batchSize = 50)
		{
			var theRefs = refs.ToList();
			var seqPromise = new SequencePromise<IContentObject>(theRefs.Count);

			var x = ContentApi.Instance.FlatMap<SequencePromise<IContentObject>, IList<IContentObject>>(api =>
			{
				var promiseGenerators =
					theRefs.Select(r => new Func<Promise<IContentObject>>(() => api.GetContent(r))).ToList();
				var seq = Promise.ExecuteInBatchSequence(batchSize, promiseGenerators, () => !Application.isPlaying);
				seq.OnElementSuccess(seqPromise.ReportEntrySuccess);
				seq.OnElementError(seqPromise.ReportEntryError);

				return seq;
			}, () => seqPromise);

			return x;
		}
	}

	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class ContentRef : BaseContentRef
	{
		private readonly Type _contentType;
		private string _id;

		public ContentRef(Type contentType, string id)
		{
			_contentType = contentType;
			_id = id;
		}

		public override string GetId() => _id;

		public override void SetId(string id) => _id = id;

		public override bool IsContent(IContentObject content)
		{
			return content.Id.Equals(_id);
		}

		public override Type GetReferencedType()
		{
			if (string.IsNullOrEmpty(_id))
				return _contentType;
			return ContentTypeReflectionCache.Instance.GetTypeFromId(_id);
		}

		public override Type GetReferencedBaseType()
		{
			return _contentType;
		}
	}

	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	/// <typeparam name="TContent"></typeparam>
	[System.Serializable]
	public class ContentRef<TContent> : AbsContentRef<TContent> where TContent : ContentObject, IContentObject, new()
	{
		public override Promise<TContent> Resolve(string manifestID = "")
		{
			var api = ContentApi.Instance;

			if (api.IsCompleted)
			{
				return api.GetResult().GetContent(this, manifestID);
			}
			else
			{
				return api.FlatMap(service => service.GetContent(this, manifestID));
			}

		}

		public override string ToString()
		{
			return $"Type={GetType().Name} Id=[{Id}]";
		}
	}

	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	/// <typeparam name="TContent"></typeparam>
	[System.Serializable]
	public class ContentLink<TContent> : AbsContentLink<TContent> where TContent : ContentObject, IContentObject, new()
	{
		private Promise<TContent> _promise;

		public override Promise<TContent> Resolve(string manifestID = "")
		{
			return _promise ?? (_promise = ContentApi.Instance.FlatMap(service => service.GetContent(this, manifestID)));
		}

		public override void OnCreated()
		{
			if (Application.isPlaying)
			{
				Resolve();
			}
		}
	}
}
