using Beamable.Common.Content;
using System;
using UnityEngine;

namespace Beamable.Common.Api.Content
{
	public interface IContentApi : ISupportsGet<ClientManifest>
	{
		/// <summary>
		/// Get the content data for a requested content id.
		/// The content is stored on Beamable servers and cached on each player's device. This method will always get the <i>latest</i>
		/// version of the content. Each time there is a new version, this method will cause a network call. Once the version has been
		/// cached, this method will read from the cache.
		/// </summary>
		/// <param name="contentId">
		/// A content id should be a dot separated string, with at least 1 dot.
		/// The right-most clause represents the content's <i>name</i>, and everything else represents the content's <i>type</i>.
		/// </param>
		/// <param name="manifestID">
		/// By default, use the "global" manifest. A realm can have multiple groupings of content, where each group has its own manifest.
		/// If you haven't published multiple manifests, you should not use this field.
		/// </param>
		/// <returns>
		/// A <see cref="Promise{IContentObject}"/> that returns a <see cref="IContentObject"/> for the given content id.
		/// </returns>
		Promise<IContentObject> GetContent(string contentId, string manifestID = "");

		/// <summary>
		/// Get the content data for a requested content id.
		/// The content is stored on Beamable servers and cached on each player's device. This method will always get the <i>latest</i>
		/// version of the content. Each time there is a new version, this method will cause a network call. Once the version has been
		/// cached, this method will read from the cache.
		/// </summary>
		/// <param name="contentId">
		/// A content id should be a dot separated string, with at least 1 dot.
		/// The right-most clause represents the content's <i>name</i>, and everything else represents the content's <i>type</i>.
		/// </param>
		/// <param name="contentType">
		/// If you know the type of the content, you can pass it so that the resulting <see cref="IContentObject"/> will be
		/// castable to the given <see cref="contentType"/>
		/// </param>
		/// <param name="manifestID">
		/// By default, use the "global" manifest. A realm can have multiple groupings of content, where each group has its own manifest.
		/// If you haven't published multiple manifests, you should not use this field.
		/// </param>
		/// <returns>
		/// A <see cref="Promise{IContentObject}"/> that returns a <see cref="IContentObject"/> for the given content id
		/// </returns>
		Promise<IContentObject> GetContent(string contentId, Type contentType, string manifestID = "");

		/// <summary>
		/// Get the content data for a requested <see cref="IContentRef"/>.
		/// The content is stored on Beamable servers and cached on each player's device. This method will always get the <i>latest</i>
		/// version of the content. Each time there is a new version, this method will cause a network call. Once the version has been
		/// cached, this method will read from the cache.
		/// </summary>
		/// <param name="reference">
		/// A <see cref="IContentRef"/> that contains a fully qualified content id.
		/// </param>
		/// <param name="manifestID">
		/// By default, use the "global" manifest. A realm can have multiple groupings of content, where each group has its own manifest.
		/// If you haven't published multiple manifests, you should not use this field.
		/// </param>
		/// <returns>
		/// A <see cref="Promise{IContentObject}"/> that returns a <see cref="IContentObject"/> for the given content reference
		/// </returns>
		Promise<IContentObject> GetContent(IContentRef reference, string manifestID = "");

		/// <summary>
		/// Get the content data for a requested <see cref="IContentRef"/>.
		/// The content is stored on Beamable servers and cached on each player's device. This method will always get the <i>latest</i>
		/// version of the content. Each time there is a new version, this method will cause a network call. Once the version has been
		/// cached, this method will read from the cache.
		/// </summary>
		/// <param name="reference">
		/// A <see cref="IContentRef"/> that contains a fully qualified content id.
		/// </param>
		/// <param name="manifestID">
		/// By default, use the "global" manifest. A realm can have multiple groupings of content, where each group has its own manifest.
		/// If you haven't published multiple manifests, you should not use this field.
		/// </param>
		/// <typeparam name="TContent">The type of the content that the <see cref="reference"/> refers to.</typeparam>
		/// <returns>
		/// A <see cref="Promise{TContent}"/> that returns a <see cref="TContent"/> for the given content reference
		/// </returns>
		Promise<TContent> GetContent<TContent>(IContentRef reference, string manifestID = "") where TContent : ContentObject, new();

		/// <summary>
		/// Get the content data for a requested <see cref="IContentRef{TContent}"/>.
		/// The content is stored on Beamable servers and cached on each player's device. This method will always get the <i>latest</i>
		/// version of the content. Each time there is a new version, this method will cause a network call. Once the version has been
		/// cached, this method will read from the cache.
		/// </summary>
		/// <param name="reference">
		/// A <see cref="IContentRef"/> that contains a fully qualified content id.
		/// </param>
		/// <param name="manifestID">
		/// By default, use the "global" manifest. A realm can have multiple groupings of content, where each group has its own manifest.
		/// If you haven't published multiple manifests, you should not use this field.
		/// </param>
		/// <typeparam name="TContent">The type of the content that the <see cref="reference"/> refers to.</typeparam>
		/// <returns>
		/// A <see cref="Promise{TContent}"/> that returns a <see cref="TContent"/> for the given content reference
		/// </returns>
		Promise<TContent> GetContent<TContent>(IContentRef<TContent> reference, string manifestID = "") where TContent : ContentObject, new();

		/// <summary>
		/// A <see cref="ClientManifest"/> describes all the content available in a manifest for a realm.
		/// This method will always start a network request to get the manifest from Beamable.
		/// </summary>
		/// <param name="manifestID">
		/// By default, use the "global" manifest. A realm can have multiple groupings of content, where each group has its own manifest.
		/// If you haven't published multiple manifests, you should not use this field.
		/// </param>
		/// <returns>A <see cref="Promise{ClientManifest}"/> that will complete when the latest <see cref="ClientManifest"/> has been received from Beamable.</returns>
		Promise<ClientManifest> GetManifestWithID(string manifestID = "");

		/// <summary>
		/// A <see cref="ClientManifest"/> describes all the content available in a manifest for a realm.
		/// This method will always start a network request to get the manifest from Beamable.
		/// </summary>
		/// <param name="filter">A string version of a <see cref="ContentQuery"/> that will be used to down filter the resulting <see cref="ClientContentInfo"/> entries in the <see cref="ClientManifest"/></param>
		/// <param name="manifestID">
		/// By default, use the "global" manifest. A realm can have multiple groupings of content, where each group has its own manifest.
		/// If you haven't published multiple manifests, you should not use this field.
		/// </param>
		/// <returns>
		/// A <see cref="Promise{ClientManifest}"/> that will complete when the latest <see cref="ClientManifest"/> has been received from Beamable.
		/// The manifest will only include entries that passed the given <see cref="filter"/>
		/// </returns>
		Promise<ClientManifest> GetManifest(string filter = "", string manifestID = "");

		/// <summary>
		/// A <see cref="ClientManifest"/> describes all the content available in a manifest for a realm.
		/// This method will always start a network request to get the manifest from Beamable.
		/// </summary>
		/// <param name="query">A <see cref="ContentQuery"/> that will be used to down filter the resulting <see cref="ClientContentInfo"/> entries in the <see cref="ClientManifest"/></param>
		/// <param name="manifestID">
		/// By default, use the "global" manifest. A realm can have multiple groupings of content, where each group has its own manifest.
		/// If you haven't published multiple manifests, you should not use this field.
		/// </param>
		/// <returns>
		/// A <see cref="Promise{ClientManifest}"/> that will complete when the latest <see cref="ClientManifest"/> has been received from Beamable.
		/// The manifest will only include entries that passed the given <see cref="filter"/>
		/// </returns>
		Promise<ClientManifest> GetManifest(ContentQuery query, string manifestID = "");
	}

	public static class ContentApi
	{
		// TODO: This is very hacky, but it lets use inject a different service in. Replace with ServiceManager (lot of unity deps to think about)
		public static Promise<IContentApi> Instance = new Promise<IContentApi>();

#if UNITY_2019_3_OR_NEWER
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void HandleDomainReset()
		{
			Instance = new Promise<IContentApi>();
		}
#endif
	}
}
