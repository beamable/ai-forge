using Beamable.Common.Api.Inventory;
using Beamable.Common.Inventory;
using Beamable.Common.Pooling;
using Beamable.Content.Utility;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Beamable.Common.Api.Mail
{
	public abstract class AbsMailApi : IMailApi
	{
		protected IBeamableRequester Requester { get; }
		protected IUserContext Ctx { get; }
		public const string SERVICE_NAME = "mail";

		protected AbsMailApi(IBeamableRequester requester, IUserContext ctx)
		{
			Requester = requester;
			Ctx = ctx;
		}

		public Promise<SearchMailResponse> SearchMail(SearchMailRequest request)
		{
			var url = $"/object/mail/{Ctx.UserId}/search";

			using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
			{
				var dict = request.Serialize();
				var json = Json.Serialize(dict, pooledBuilder.Builder);
				return Requester.Request<SearchMailResponse>(Method.POST, url, json);
			}
		}

		public Promise<ListMailResponse> GetMail(string category, long startId = 0, long limit = 100)
		{
			const string key = "search";
			var req = new SearchMailRequest(
			  new SearchMailRequestClause
			  {
				  name = key,
				  categories = new[] { category },
				  states = new[] { "Read", "Unread" },
				  limit = limit,
				  start = startId > 0 ? (long?)(startId) : null
			  }
			);
			return SearchMail(req).Map(res =>
			{
				var content = res.results.Find(set => set.name == key)?.content;
				return new ListMailResponse
				{
					result = content
				};
			});
		}


		public Promise<EmptyResponse> SendMail(MailSendRequest request)
		{
			return Requester.Request<EmptyResponse>(
			  Method.POST,
			  $"/basic/mail/bulk",
			  request
			);
		}

		public Promise<EmptyResponse> Update(MailUpdateRequest updates)
		{
			return Requester.Request<EmptyResponse>(
			  Method.PUT,
			  $"/object/mail/{Ctx.UserId}/bulk",
			  updates
			);
		}

		/// <summary>
		/// Accept all the attachments from a set of mail messages.
		/// </summary>
		/// <param name="manyRequest">Request structure containing numeric message IDs.</param>
		public Promise<EmptyResponse> AcceptMany(MailAcceptManyRequest manyRequest)
		{
			return Requester.Request<EmptyResponse>(
			  Method.PUT,
			  $"/object/mail/{Ctx.UserId}/accept/many",
			  manyRequest
			);
		}

		public abstract Promise<MailQueryResponse> GetCurrent(string scope = "");
	}


	[Serializable]
	public class MailQueryResponse
	{
		public int unreadCount;
	}

	[Serializable]
	public class ListMailResponse
	{
		public List<MailMessage> result;
	}

	[Serializable]
	public class SearchMailRequest
	{
		public SearchMailRequestClause[] clauses;

		public SearchMailRequest(params SearchMailRequestClause[] clauses)
		{
			this.clauses = clauses;
		}

		public ArrayDict Serialize()
		{
			var serializedClauses = new ArrayDict[clauses.Length];
			for (var i = 0; i < serializedClauses.Length; i++)
			{
				serializedClauses[i] = clauses[i].Serialize();
			}

			return new ArrayDict
	  {
		{nameof(clauses), serializedClauses}
	  };
		}
	}

	/// <summary>
	/// A way to specify certain criteria for searching mail.
	/// Each instance of <see cref="SearchMailRequestClause"/> can specify different types of mail to identify.
	/// Each <see cref="SearchMailRequestClause"/> will result in a corresponding <see cref="SearchMailResponseClause"/>
	/// </summary>
	[Serializable]
	public class SearchMailRequestClause
	{
		/// <summary>
		/// The name of the clause must be unique, and should describe the filter this particular clause performs.
		/// When the mail request clause is processed, it will produce a <see cref="SearchMailResponseClause"/>. The resulting
		/// <see cref="SearchMailResponseClause.name"/> field will match the value of this <see cref="name"/> field.
		/// </summary>
		public string name;

		/// <summary>
		/// When true, the <see cref="SearchMailResponseClause"/> will only count the number of mail objects that met the <see cref="SearchMailRequestClause"/>,
		/// and the actual <see cref="SearchMailResponseClause.content"/> field will be empty.
		/// </summary>
		public bool onlyCount;

		/// <summary>
		/// The categories of mail to include in this filter clause. A category can be any string.
		/// </summary>
		public string[] categories;

		/// <summary>
		/// The various states of mail to include in this filter clause.
		/// Valid states include, "Unread", "Read", and "Deleted"
		/// </summary>
		public string[] states;

		/// <summary>
		/// An optional player id that selects mail coming from the specific player.
		/// </summary>
		public long? forSender;

		/// <summary>
		/// An optional maximum number of <see cref="MailMessage"/> to accept in the resulting <see cref="SearchMailResponseClause.content"/> list.
		/// This will also max out the <see cref="SearchMailResponseClause.count"/> field.
		/// This can be used with the <see cref="start"/> field to page the player's mail.
		/// </summary>
		public long? limit;

		/// <summary>
		/// An optional offset into the players mail. This can be used with the <see cref="limit"/> field to page the player's mail.
		/// </summary>
		public long? start;

		public ArrayDict Serialize()
		{
			var dict = new ArrayDict();

			dict.Add(nameof(name), name);
			dict.Add(nameof(onlyCount), onlyCount);

			if (categories != null)
			{
				dict.Add(nameof(categories), categories);
			}

			if (states != null)
			{
				dict.Add(nameof(states), states);
			}

			if (limit.HasValue)
			{
				dict.Add(nameof(limit), limit.Value);
			}

			if (forSender.HasValue)
			{
				dict.Add(nameof(forSender), forSender.Value);
			}

			if (start.HasValue)
			{
				dict.Add(nameof(start), start.Value);
			}

			return dict;
		}
	}


	[Serializable]
	public class SearchMailResponse
	{
		public List<SearchMailResponseClause> results;
	}

	/// <summary>
	/// Each instance of <see cref="SearchMailResponseClause"/> aligns with an original <see cref="SearchMailRequestClause"/> instance.
	/// The <see cref="SearchMailResponseClause"/> contain the matching <see cref="MailMessage"/>s that met the criteria defined in the
	/// request.
	/// </summary>
	[Serializable]
	public class SearchMailResponseClause
	{
		/// <summary>
		/// The number of matching <see cref="MailMessage"/>.
		/// This will always be equal to the size of the <see cref="MailMessage"/> list,
		/// unless the original <see cref="SearchMailRequestClause.onlyCount"/> field was set to true.
		/// </summary>
		public int count;

		/// <summary>
		/// The name of the original <see cref="SearchMailRequestClause.name"/>
		/// </summary>
		public string name;

		/// <summary>
		/// The set of <see cref="MailMessage"/> that met the criteria.
		/// If the original <see cref="SearchMailRequestClause.onlyCount"/> field was set to true, this list will be empty.
		/// </summary>
		public List<MailMessage> content;
	}

	[Serializable]
	public class MailMessage
	{
		/// <summary>
		/// The instance id of the mail
		/// </summary>
		public long id;

		/// <summary>
		/// The timestamp that the message was originally sent
		/// </summary>
		public long sent;

		/// <summary>
		/// The timestamp that the message was claimed for rewards.
		/// The number of milliseconds from 1970-01-01T00:00:00Z.
		/// </summary>
		public long claimedTimeMs;

		/// <summary>
		/// The gamertag of the player who received the mail
		/// </summary>
		public long receiverGamerTag;

		/// <summary>
		/// The gamertag of the player who sent the mail
		/// </summary>
		public long senderGamerTag;

		/// <summary>
		/// The category of the mail
		/// </summary>
		public string category;

		/// <summary>
		/// The subject line of the mail
		/// </summary>
		public string subject;

		/// <summary>
		/// The body of the mail
		/// </summary>
		public string body;

		/// <summary>
		/// The state of the mail.
		/// Valid states include, "Unread", "Read", and "Deleted"
		/// </summary>
		public string state;

		/// <summary>
		/// An optional date-string that represents when the mail will be automatically removed.
		/// </summary>
		public string expires;

		/// <summary>
		/// The <see cref="MailRewards"/> associated with this mail
		/// </summary>
		public MailRewards rewards;

		public MailState MailState => (MailState)Enum.Parse(typeof(MailState), state);
	}

	[Serializable]
	public class MailCounts
	{
		public long sent;
		public MailStateCounts received;
	}

	[Serializable]
	public class MailStateCounts
	{
		public long all;
		public long unread;
		public long read;
		public long deleted;
	}

	[Serializable]
	public class MailGetCountsResponse
	{
		public MailCounts total;
	}

	[Serializable]
	public class MailSendRequest
	{
		public List<MailSendEntry> sendMailRequests = new List<MailSendEntry>();

		public MailSendRequest Add(MailSendEntry entry)
		{
			sendMailRequests.Add(entry);
			return this;
		}
	}

	[Serializable]
	public class MailSendEntry
	{
		public long senderGamerTag;
		public long receiverGamerTag;
		public string category;
		public string subject;
		public string body;
		public string expires;
		public MailRewards rewards;

		/// <summary>
		/// Sets the mail expiration based on the iso date time format (yyyy-MM-ddTHH:mm:ssZ)
		/// </summary>
		/// <param name="expirationIsoDateTime"></param>
		/// <returns></returns>
		public MailSendEntry SetExpiration(string expirationIsoDateTime)
		{
			if (expirationIsoDateTime != null)
			{
				var date = DateTimeOffset.ParseExact(expirationIsoDateTime, DateUtility.ISO_FORMAT, CultureInfo.InvariantCulture,
													 DateTimeStyles.None);
				expires = date.ToUniversalTime().ToString(DateUtility.ISO_FORMAT);
			}

			return this;
		}

		/// <summary>
		/// Sets the mail expiration based on a specified unix timestamp (milliseconds)
		/// </summary>
		/// <param name="expirationTimestampMillis"></param>
		/// <returns></returns>
		public MailSendEntry SetExpiration(long expirationTimestampMillis)
		{
			expires = DateTimeOffset.FromUnixTimeMilliseconds(expirationTimestampMillis).ToString(DateUtility.ISO_FORMAT);
			return this;
		}

		/// <summary>
		/// Sets the mail expiration based on a specified Date Time (UTC)
		/// </summary>
		/// <param name="expirationDateTime"></param>
		/// <returns></returns>
		public MailSendEntry SetExpiration(DateTimeOffset expirationDateTime)
		{
			expires = expirationDateTime.ToUniversalTime().ToString(DateUtility.ISO_FORMAT);
			return this;
		}

		/// <summary>
		/// Sets the mail expiration based on a timespan relative to the current date time (e.g. in 10 minutes)
		/// </summary>
		/// <param name="expiresInTimespan"></param>
		/// <returns></returns>
		public MailSendEntry SetExpiresIn(TimeSpan expiresInTimespan)
		{
			expires = DateTimeOffset.UtcNow.Add(expiresInTimespan).ToString(DateUtility.ISO_FORMAT);
			return this;
		}
	}

	[Serializable]
	public class MailRewards
	{
		/// <summary>
		/// Updates to player currencies
		/// </summary>
		public List<CurrencyChange> currencies;

		/// <summary>
		/// New items for the player
		/// </summary>
		public List<ItemCreateRequest> items;

		/// <summary>
		/// When true, any <see cref="currencies"/> will apply their VIP bonus.
		/// </summary>
		public bool applyVipBonus = true;
	}

	[Serializable]
	public class MailUpdate
	{
		public long mailId;
		public string state;
		public string expires;
		public bool acceptAttachments;

		public MailUpdate(long mailId, MailState state, bool acceptAttachments, string expires)
		{
			this.mailId = mailId;
			this.state = state.ToString();
			this.acceptAttachments = acceptAttachments;
			this.expires = expires;
		}

		public MailUpdate(long mailId, MailState state, bool acceptAttachments)
		{
			this.mailId = mailId;
			this.state = state.ToString();
			this.acceptAttachments = acceptAttachments;
		}

		/// <summary>
		/// Sets the mail expiration based on the iso date time format (yyyy-MM-ddTHH:mm:ssZ)
		/// </summary>
		/// <param name="expirationIsoDateTime"></param>
		/// <returns></returns>
		public MailUpdate SetExpiration(string expirationIsoDateTime)
		{
			if (expirationIsoDateTime != null)
			{
				var date = DateTimeOffset.ParseExact(expirationIsoDateTime, DateUtility.ISO_FORMAT, CultureInfo.InvariantCulture,
													 DateTimeStyles.None);
				expires = date.ToUniversalTime().ToString(DateUtility.ISO_FORMAT);
			}

			return this;
		}

		/// <summary>
		/// Sets the mail expiration based on a specified unix timestamp (milliseconds)
		/// </summary>
		/// <param name="expirationTimestampMillis"></param>
		/// <returns></returns>
		public MailUpdate SetExpiration(long expirationTimestampMillis)
		{
			expires = DateTimeOffset.FromUnixTimeMilliseconds(expirationTimestampMillis).ToString(DateUtility.ISO_FORMAT);
			return this;
		}

		/// <summary>
		/// Sets the mail expiration based on a specified Date Time (UTC)
		/// </summary>
		/// <param name="expirationDateTimeOffset"></param>
		/// <returns></returns>
		public MailUpdate SetExpiration(DateTimeOffset expirationDateTimeOffset)
		{
			expires = expirationDateTimeOffset.ToUniversalTime().ToString(DateUtility.ISO_FORMAT);
			return this;
		}

		/// <summary>
		/// Sets the mail expiration based on a timespan relative to the current date time (e.g. in 10 minutes)
		/// </summary>
		/// <param name="expiresInTimespan"></param>
		/// <returns></returns>
		public MailUpdate SetExpiresIn(TimeSpan expiresInTimespan)
		{
			expires = DateTimeOffset.UtcNow.Add(expiresInTimespan).ToString(DateUtility.ISO_FORMAT);
			return this;
		}
	}

	[Serializable]
	public class MailUpdateEntry
	{
		public long id;
		public MailUpdate update;
	}

	[Serializable]
	public class MailUpdateRequest
	{
		public List<MailUpdateEntry> updateMailRequests = new List<MailUpdateEntry>();

		public MailUpdateRequest Add(long id, MailUpdate mailUpdate)
		{
			updateMailRequests.Add(new MailUpdateEntry { id = id, update = mailUpdate });
			return this;
		}

		public MailUpdateRequest Add(long id, MailState state, bool acceptAttachments)
		{
			return Add(id, new MailUpdate(id, state, acceptAttachments));
		}

		public MailUpdateRequest Add(long id, MailState state, bool acceptAttachments, string expires)
		{
			return Add(id, new MailUpdate(id, state, acceptAttachments).SetExpiration(expires));
		}

		public MailUpdateRequest Add(long id, MailState state, bool acceptAttachments, long expirationTimestampMillis)
		{
			return Add(id, new MailUpdate(id, state, acceptAttachments).SetExpiration(expirationTimestampMillis));
		}

		public MailUpdateRequest Add(long id, MailState state, bool acceptAttachments, DateTimeOffset expirationDateTimeOffset)
		{
			return Add(id, new MailUpdate(id, state, acceptAttachments).SetExpiration(expirationDateTimeOffset));
		}

		public MailUpdateRequest Add(long id, MailState state, bool acceptAttachments, TimeSpan expiresIn)
		{
			return Add(id, new MailUpdate(id, state, acceptAttachments).SetExpiresIn(expiresIn));
		}
	}

	[Serializable]
	public class MailReceivedRequest
	{
		public string[] categories;
		public string[] states;
		public long limit;
	}

	[Serializable]
	public class MailCountRequest
	{
		public string[] categories;
	}

	[Serializable]
	public class MailAcceptManyRequest
	{
		public long[] mailIds;
	}

	public enum MailState
	{
		Read,
		Unread,
		Claimed,
		Deleted
	}
}
