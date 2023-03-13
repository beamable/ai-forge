using System;
using System.Collections.Generic;

namespace Beamable.Common.Api.CloudData
{
	/// <summary>
	/// A message containing the player's <see cref="CloudMetaData"/> data, in the <see cref="meta"/> list
	/// </summary>
	[Serializable]
	public class GetCloudDataManifestResponse
	{
		/// <summary>
		/// A message describing the state of the message. If the message was received correctly, the value will be "ok"
		/// </summary>
		public string result;

		/// <summary>
		/// A list of cloud data entries
		/// </summary>
		public List<CloudMetaData> meta;
	}

	/// <summary>
	/// A CloudMetaData represents a piece of game data that can be overridden by the server, or by specific cohorts for players.
	/// This structure is the metadata about the game data itself. You can use the <see cref="uri"/> field to resolve the data.
	/// </summary>
	[Serializable]
	public class CloudMetaData
	{
		/// <summary>
		/// The unique id of this particular cloud data reference
		/// </summary>
		public long sid;

		/// <summary>
		/// The version of the cloud data reference. If you update the cloud data from the portal, this version increases.
		/// </summary>
		public long version;

		/// <summary>
		/// The name of the base game cloud data that this player cloud data refers to. Every player cloud data must be attached to a base game piece of data.
		/// </summary>
		public string @ref;

		/// <summary>
		/// A uri that points to the data for the cloud data. You can use the <see cref="IHttpRequester"/> to request this data.
		/// </summary>
		public string uri;

		/// <summary>
		/// The player may be in a special cohort that changes the default uri and resulting data.
		/// The <see cref="CohortEntry.trial"/> is <inheritdoc cref="CohortEntry.trial"/>
		/// The <see cref="CohortEntry.cohort"/> is <inheritdoc cref="CohortEntry.cohort"/>
		/// </summary>
		public CohortEntry cohort;

		public bool IsDefault => string.IsNullOrEmpty(cohort?.trial) && string.IsNullOrEmpty(cohort?.cohort);
	}

	[Serializable]
	public class CohortEntry
	{
		/// <summary>
		/// The name of the trial. Blank if there is no trial. A trial can have many cohorts.
		/// </summary>
		public string trial;

		/// <summary>
		/// the name of the cohort. Blank if there is no assigned cohort.
		/// </summary>
		public string cohort;
	}

	/// <summary>
	/// This type defines the %Client main entry point for the %A/B %Testing feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/ab-testing-feature-overview">A/B Testing</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class CloudDataApi : ICloudDataApi
	{
		public IUserContext Ctx { get; }
		public IBeamableRequester Requester { get; }

		public CloudDataApi(IUserContext ctx, IBeamableRequester requester)
		{
			Ctx = ctx;
			Requester = requester;
		}

		public Promise<GetCloudDataManifestResponse> GetGameManifest()
		{
			return Requester.Request<GetCloudDataManifestResponse>(
			   Method.GET,
			   "/basic/cloud/meta"
			);
		}

		public Promise<GetCloudDataManifestResponse> GetPlayerManifest()
		{
			return Requester.Request<GetCloudDataManifestResponse>(
			   Method.GET,
			   "/basic/cloud/meta/player/all"
			);
		}

		public Promise<string> GetCloudDataContent(CloudMetaData metaData)
		{
			return Requester.Request(Method.GET,
									 $"https://{metaData.uri}", parser: s => s);
		}
	}
}
