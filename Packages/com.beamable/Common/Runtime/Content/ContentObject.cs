using Beamable.Common.Content.Serialization;
using Beamable.Common.Content.Validation;
using Beamable.Content;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Beamable.Common.Content
{
	/// <summary>
	/// This type defines the parent of all %Beamable %ContentObjects for the %ContentService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Direct Subclasses
	/// - See Beamable.Common.Announcements.AnnouncementContent script reference
	/// - See Beamable.Common.Content.EmailContent script reference
	/// - See Beamable.Common.Content.EventContent script reference
	/// - See Beamable.Common.Content.SimGameType script reference
	/// - See Beamable.Common.Groups.GroupDonationsContent  script reference
	/// - See Beamable.Common.Inventory.CurrencyContent script reference
	/// - See Beamable.Common.Inventory.ItemContent script reference
	/// - See Beamable.Common.Inventory.VipContent script reference
	/// - See Beamable.Common.Leaderboards.LeaderboardContent script reference
	/// - See Beamable.Common.Shop.ListingContent  script reference
	/// - See Beamable.Common.Shop.SKUContent script reference
	/// - See Beamable.Common.Shop.StoreContent  script reference
	/// - See Beamable.Common.Tournaments.TournamentContent script reference
	/// - See Beamable.Experimental.Common.Calendars.CalendarContent script reference
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-feature">Content</a> feature documentation
	/// - See Beamable.Content.ContentService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[System.Serializable]
	public class ContentObject : ScriptableObject, IContentObject, IRawJsonProvider
	{
		// ////////////////////////////////////////////////////////
		// Reusable text for within ContentObject subclasses
		// for [Tooltip] attributes.
		// ////////////////////////////////////////////////////////

		// Debugging
		// Seeing this text in the unity editor indicates
		// there probably **IS** a [Tooltip] but
		// Some custom PropertyDrawer does not yet respect it. If seen,
		// then determine which custom PropertyDrawer must be updated
		// and be sure to manually pass along the [Tooltip] to be rendered
		public const string TooltipNotFoundDebugFallback1a = "(No Tooltip)";

		// 0 - Optional things that come first
		internal const string TooltipDivider = " - ";
		internal const string TooltipOptional0 = "(Optional) "; //keep trailing space

		// 1 Prepends
		internal const string ButtonText1 = "The button txt";
		internal const string DefaultValue1 = "The default value";
		internal const string TooltipActivePeriod1 = "The active period";
		internal const string TooltipAllowedCurrency1 = "The allowed currency";
		internal const string TooltipAmount1 = "The amount";
		internal const string TooltipAttachment1 = "The content that players can claim";
		internal const string TooltipBody1 = "The body";
		internal const string TooltipClientPermission1 = "Determines write access";
		internal const string TooltipCohort1 = "The cohort";
		internal const string TooltipCohortTrial1 = "The cohort trial";
		internal const string TooltipColor1 = "The color";
		internal const string TooltipConstraint1 = "The constraint";
		internal const string TooltipCount1 = "The count";
		internal const string TooltipCurrency1 = "The currency";
		internal const string TooltipCurrencyAmount1 = "The currency amount";
		internal const string TooltipCycle1 = "The cycle";
		internal const string TooltipDelta1 = "The delta";
		internal const string TooltipDeltaMax1 = "The maximum delta";
		internal const string TooltipDescription1 = "The description";
		internal const string TooltipDisqualifyThreshold1 = "The disqualify threshold";
		internal const string TooltipDurationMinutes1 = "The duration in minutes";
		internal const string TooltipDurationPurchaseLimit1 = "The duration purchase limit";
		internal const string TooltipDurationSeconds1 = "The duration in seconds";
		internal const string TooltipEndDate1 = "The end date, defined as UTC";
		internal const string TooltipEndRank1 = "The end rank";
		internal const string TooltipEventItemObtain1 = "The event item obtain";
		internal const string TooltipEventRule1 = "The event rule";
		internal const string TooltipEventSchedule = "The schedule for when the event repeats";
		internal const string TooltipFederation = "Federated service that content item will be attached to";
		internal const string TooltipGroupReward1 = "The group reward";
		internal const string TooltipIcon1 = "The icon image";
		internal const string TooltipId1 = "The id";
		internal const string TooltipIncrement1 = "The increment";
		internal const string TooltipKey1 = "The key";
		internal const string TooltipLeaderboard1 = "The leaderboard";
		internal const string TooltipLeaderboardUpdate1 = "The leaderboard update";
		internal const string TooltipListing1 = "The listing";
		internal const string TooltipListingOffer1 = "The listing offer";
		internal const string TooltipListingPrice1 = "The listing price";
		internal const string TooltipMatchmakingRule1 = "The matchmaking rule";
		internal const string TooltipMaximum1 = "The maximum";
		internal const string TooltipMinimum1 = "The minimum";
		internal const string TooltipMultiplier1 = "Multiplier";
		internal const string TooltipName1 = "The name";
		internal const string TooltipObtainCurrency1 = "The currency to be obtained";
		internal const string TooltipObtainItem1 = "The item to be obtained";
		internal const string TooltipPartitionSize1 = "The partition size";
		internal const string TooltipPhase1 = "The phase";
		internal const string TooltipPlayerDbid1 = "The player id (Dbid)";
		internal const string TooltipPlayersMax1 = "The maximum players";
		internal const string TooltipPlayersMin1 = "The minimum players";
		internal const string TooltipProperty1 = "The property";
		internal const string TooltipPurchase1 = "The purchase";
		internal const string TooltipPurchaseLimit1 = "The purchase limit";
		internal const string TooltipQualifyThreshold1 = "The qualify threshold";
		internal const string TooltipRankMax1 = "The rank maximum";
		internal const string TooltipRankMin1 = "The rank minimum";
		internal const string TooltipRankReward1 = "The rank reward";
		internal const string TooltipRequestCooldown1 = "The request cooldown";
		internal const string TooltipResult1 = "The result";
		internal const string TooltipReward1 = "The reward";
		internal const string TooltipRewardType1 = "The reward type";
		internal const string TooltipRewardsPerRank1 = "The rewards per rank";
		internal const string TooltipRoundToNearest1 = "Round to nearest";
		internal const string TooltipRule1 = "The rule";
		internal const string TooltipSKUProductIds1 = "The sku product ids";
		internal const string TooltipScore1 = "The score";
		internal const string TooltipScoreMax1 = "The score maximum";
		internal const string TooltipScoreMin1 = "The score minimum";
		internal const string TooltipScoreReward1 = "The score reward";
		internal const string TooltipScoringAlgorithm1 = "The scoring algorithm";
		internal const string TooltipScoringAlgorithmOption1 = "The scoring algorithm option";
		internal const string TooltipSecondsRemaining1 = "The time remaining in seconds";
		internal const string TooltipShowInactive1 = "Show innactive?";
		internal const string TooltipStage1 = "The stage";
		internal const string TooltipStageMax1 = "The stage maximum";
		internal const string TooltipStageMin1 = "The stage minimum";
		internal const string TooltipStartDate1 = "The start date, defined as UTC";
		internal const string TooltipStartRank1 = "The start rank";
		internal const string TooltipStat1 = "The stat";
		internal const string TooltipStatus = "The status";
		internal const string TooltipStore1 = "The store";
		internal const string TooltipSubject1 = "The subject";
		internal const string TooltipSymbol1 = "The symbol";
		internal const string TooltipTeamContent1 = "The team content";
		internal const string TooltipTheListOf1 = "The list of ";
		internal const string TooltipTier1 = "The tier";
		internal const string TooltipTitle1 = "The title";
		internal const string TooltipTournamentInfo1 = "The tournament info";
		internal const string TooltipTournamentRankReward1 = "The tournament rank reward";
		internal const string TooltipTournamentRewardCurrency1 = "The tournament reward currency";
		internal const string TooltipType1 = "The type";
		internal const string TooltipValue1 = "The value";
		internal const string TooltipVipBonus1 = "The bonus";
		internal const string TooltipVipTier1 = "The vip tier";

		internal const string TooltipWaitAfterMinReachedSecs1 =
		   "The wait time after the minimum players is reached, in seconds.";

		internal const string TooltipWaitDurationSecsMax1 = "The maximum wait time in seconds";
		internal const string TooltipMatchingIntervalSecs1 = "The rate by which the matchmaking system finds matches. Defaults to 10 seconds.";
		internal const string WriteSelf1 = "True, content is writeable via client C# and server. False, only via server";
		internal const string TooltipScheduleInstancePurchaseLimit = "The schedule instance purchase limit";

		// 2 Postpends (Keep leading spaces)
		internal const string TooltipActive2 = " active";
		internal const string TooltipActiveCooldown2 = " of active cooldown";
		internal const string TooltipEndDate2 = TooltipStartDate2; //same, for now
		internal const string TooltipRequirement2 = " requirement";
		internal const string TooltipStartDate2 = " - Format is YYYY:MM:DD:HH:MM:SS";

		/// <summary>
		/// Invoked after %ContentObject field values change
		/// </summary>
		public event ContentDelegate OnChanged;

		[Obsolete] public string ContentVersion => Version;

		/// <summary>
		/// The %name of the %ContentObject
		/// </summary>
		public string ContentName { get; private set; }

		private string _contentTypeName;

		/// <summary>
		/// The %type of the %ContentObject
		/// </summary>
		public string ContentType => _contentTypeName ?? (_contentTypeName = GetContentTypeName(GetType()));

		/// <summary>
		/// The %id of the %ContentObject
		/// </summary>
		public string Id => $"{ContentType}.{ContentName}";

		/// <summary>
		/// The %Manifest %Id
		/// </summary>
		public string ManifestID { get; private set; }

		/// <summary>
		/// The %version of the %ContentObject
		/// </summary>
		public string Version { get; private set; }

		[SerializeField]
		[IgnoreContentField]
		[HideInInspector]
		private string[] _tags;

		/// <summary>
		/// The %tags of the %ContentObject
		/// </summary>
		public string[] Tags
		{
			get => _tags ?? (_tags = new[] { "base" });
			set => _tags = value;
		}

		public string Created { get; private set; }
		public long LastChanged { get; set; }
		public ContentCorruptedException ContentException { get; set; }

		/// <summary>
		/// Set the &id and &version
		/// </summary>
		/// <param name="id"></param>
		/// <param name="version"></param>
		/// <exception cref="Exception"></exception>
		public void SetIdAndVersion(string id, string version)
		{
			// validate id.
			var typeName = ContentType;
			if (typeName == null)
			{
				// somehow, the runtime type isn't available. We should infer the typeName from the id.
				typeName = ContentTypeReflectionCache.GetTypeNameFromId(id);
			}

			if (!string.Equals(_contentTypeName, typeName))
				_contentTypeName = typeName;

			if (!id.StartsWith(typeName))
			{
				throw new Exception($"Content type of [{typeName}] cannot use id=[{id}]");
			}

			SetContentName(id.Substring(typeName.Length + 1)); // +1 for the dot.

			if (!string.Equals(Version, version))
				Version = version;
		}

		/// <summary>
		/// Set the %Manifest %Id
		/// </summary>
		/// <param name="manifestID"></param>
		public void SetManifestID(string manifestID)
		{
			ManifestID = manifestID;
		}

		/// <summary>
		/// Set the %name of the %ContentObject
		/// </summary>
		/// <param name="newContentName"></param>
		/// <returns></returns>
		public ContentObject SetContentName(string newContentName)
		{
			if (!string.Equals(ContentName, newContentName))
				ContentName = newContentName;

			if (Application.isPlaying)
			{
				if (!string.Equals(name, newContentName))
					name = newContentName; // only set the SO name if we are in-game. Internally, Beamable does not depend on the SO name, but a gameMaker may want to use it.
			}

			return this;
		}


		/// <summary>
		/// Broadcast an updated changed of the %ContentObject field values
		/// </summary>
		public void BroadcastUpdate()
		{
			OnChanged?.Invoke(this);
		}


		/// <summary>
		/// Get the %TypeName of the %ContentObject
		/// </summary>
		/// <param name="contentType"></param>
		/// <returns></returns>
		public static string GetContentTypeName(Type contentType)
		{
			return ContentTypeReflectionCache.GetContentTypeName(contentType);
		}


		/// <summary>
		/// Get the %type of the %ContentObject
		/// </summary>
		/// <typeparam name="TContent"></typeparam>
		/// <returns></returns>
		public static string GetContentType<TContent>()
		   where TContent : ContentObject
		{
			return GetContentTypeName(typeof(TContent));
		}


		/// <summary>
		/// Make an instance of the %ContentObject
		/// </summary>
		/// <param name="name"></param>
		/// <typeparam name="TContent"></typeparam>
		/// <returns></returns>
		public static TContent Make<TContent>(string name)
		   where TContent : ContentObject, new()
		{
			var instance = CreateInstance<TContent>();
			instance.SetContentName(name);
			return instance;
		}


		/// <summary>
		/// Validate this `ContentObject`.
		/// </summary>
		/// <exception cref="AggregateContentValidationException">Should throw if the content is semantically invalid.</exception>
		public virtual void Validate(IValidationContext ctx)
		{
			var errors = GetMemberValidationErrors(ctx);
			if (errors.Count > 0)
			{
				throw new AggregateContentValidationException(errors);
			}
		}

#if UNITY_EDITOR

      public event Action<List<ContentException>> OnValidationChanged;
      public event Action OnEditorValidation;
      public static IValidationContext ValidationContext { get; set; }
      [IgnoreContentField] private bool _hadValidationErrors;
      public Guid ValidationGuid { get; set; }
      public static bool ShowChecksum { get; set; }
      public bool SerializeToConsoleRequested { get; set; }

      [SerializeField]
      private string _serializedValidationGUID { get; set; }
      
      private static readonly int[] _guidByteOrder =
	      new[] { 15, 14, 13, 12, 11, 10, 9, 8, 6, 7, 4, 5, 0, 1, 2, 3 };
      private static Guid Increment(Guid guid)
      {
	      var bytes = guid.ToByteArray();
	      bool carry = true;
	      for (int i = 0; i < _guidByteOrder.Length && carry; i++)
	      {
		      int index = _guidByteOrder[i];
		      byte oldValue = bytes[index]++;
		      carry = oldValue > bytes[index];
	      }
	      return new Guid(bytes);
      }

      private void OnValidate()
      {
	      if (!string.IsNullOrEmpty(_serializedValidationGUID) && Guid.TryParse(_serializedValidationGUID, out var guid))
	      {
		      ValidationGuid = guid;
	      }
	      else
	      {
		      ValidationGuid = Guid.Empty;
		      _serializedValidationGUID = ValidationGuid.ToString();
	      }

	      ValidationGuid = Increment(ValidationGuid);
	      _serializedValidationGUID = ValidationGuid.ToString();
	      
         if (ValidationContext == null)
         {
	         // if we have no validation context assigned yet, then we cannot possibly validate.
	         return;
         }
         OnEditorValidation?.Invoke();
         
         if (HasValidationExceptions(ValidationContext, out var exceptions))
         {
            _hadValidationErrors = true;
            OnValidationChanged?.Invoke(exceptions);

         }
         else if (_hadValidationErrors)
         {
            _hadValidationErrors = false;
            OnValidationChanged?.Invoke(null);
         }
      }

      public void ForceValidate()
      {
         OnValidate();

      }

      [ContextMenu("Force Validate")]
      public void LogValidationErrors()
      {
         ForceValidate();
         var ctx = ValidationContext ?? new ValidationContext();
         if (HasValidationExceptions(ctx, out var exceptions))
         {
            foreach (var ex in exceptions)
            {
               if (ex is ContentValidationException contentException)
               {
                  Debug.LogError($"Validation Failure. {Id}. {contentException.Message}");
               }
               else
               {
                  Debug.LogError($"Validation Failure. {Id}. {ex.FriendlyMessage}");

               }
            }
         }
      }

      [ContextMenu("Toggle Checksum")]
      public void ToggleShowChecksum()
      {
         ShowChecksum = !ShowChecksum;
      }

      [ContextMenu("Serialize To Console")]
      public void SerializeToConsole()
      {
         SerializeToConsoleRequested = true;
      }


#endif

		/// <summary>
		/// Determines if the %ContentObject has %Validation %Errors
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public bool HasValidationErrors(IValidationContext ctx, out List<string> errors)
		{
			errors = new List<string>();

			if (ContentName != null &&
				ContentNameValidationException.HasNameValidationErrors(this, ContentName, out var nameErrors))
			{
				errors.AddRange(nameErrors.Select(e => e.Message));
			}

			errors.AddRange(GetMemberValidationErrors(ctx)
			   .Select(e => e.Message));

			return errors.Count > 0;
		}

		/// <summary>
		/// Determines if the %ContentObject has %Validation %Exceptions
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="exceptions"></param>
		/// <returns></returns>
		public bool HasValidationExceptions(IValidationContext ctx, out List<ContentException> exceptions)
		{
			exceptions = new List<ContentException>();
			if (ContentName != null &&
				ContentNameValidationException.HasNameValidationErrors(this, ContentName, out var nameErrors))
			{
				exceptions.AddRange(nameErrors);
			}

			exceptions.AddRange(GetMemberValidationErrors(ctx));
			return exceptions.Count > 0;
		}

		/// <summary>
		/// Gets list of the %ContentObject has %Member %Validation %Errors
		/// </summary>
		public List<ContentValidationException> GetMemberValidationErrors(IValidationContext ctx)
		{
			var errors = new List<ContentValidationException>();

			var seen = new HashSet<object>();
			var toExpand = new Queue<object>();

			toExpand.Enqueue(this);
			while (toExpand.Count > 0)
			{
				var obj = toExpand.Dequeue();
				if (seen.Contains(obj))
				{
					continue;
				}

				if (obj == null) continue;


				seen.Add(obj);
				var type = obj.GetType();

				if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
				{
					var set = (IEnumerable)obj;
					foreach (var subObj in set)
					{
						toExpand.Enqueue(subObj);
					}
				}

				foreach (var field in type.GetFields())
				{
					var fieldValue = field.GetValue(obj);

					if (typeof(Optional).IsAssignableFrom(field.FieldType))
					{
						var optional = fieldValue as Optional;
						if (optional == null || !optional.HasValue)
						{
							continue;
						}
					}

					toExpand.Enqueue(fieldValue);

					foreach (var attribute in field.GetCustomAttributes<ValidationAttribute>())
					{
						try
						{
							var wrapper = new ValidationFieldWrapper(field, obj);

							if (typeof(IList).IsAssignableFrom(field.FieldType))
							{
								var value = field.GetValue(obj) as IList;
								if (value != null)
								{
									for (var i = 0; i < value.Count; i++)
									{
										attribute.Validate(ContentValidationArgs.Create(wrapper, this, ctx, i, true));
									}
								}
							}

							attribute.Validate(ContentValidationArgs.Create(wrapper, this, ctx));
						}
						catch (ContentValidationException e)
						{
							errors.Add(e);
						}
					}

				}
			}

			return errors;
		}

		/// <summary>
		/// Converts the %ContentObject to %Json
		/// </summary>
		/// <returns></returns>
		public string ToJson()
		{
			return ClientContentSerializer.SerializeContent(this);
		}
	}
	public delegate void ContentDelegate(ContentObject content);

	public delegate void IContentDelegate(IContentObject content);
	public delegate void IContentBatchDelegate(List<IContentObject> content);


	public delegate void IContentRenamedDelegate(string oldId, IContentObject content, string nextAssetPath);
}
