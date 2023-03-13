using Beamable.Common.Api.Inventory;
using Beamable.Common.Pooling;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Common.Api.Groups
{
	public class GroupsApi : IGroupsApi
	{
		public IUserContext Ctx { get; }
		public IBeamableRequester Requester { get; }

		public GroupsApi(IUserContext ctx, IBeamableRequester requester)
		{
			Ctx = ctx;
			Requester = requester;
		}
		public Promise<GroupUser> GetUser(long gamerTag)
		{
			return Requester.Request<GroupUser>(
			   Method.GET,
			   String.Format("/object/group-users/{0}", gamerTag)
			);
		}

		public Promise<Group> GetGroup(long groupId)
		{
			return Requester.Request<Group>(
			   Method.GET,
			   String.Format("/object/groups/{0}", groupId)
			);
		}

		public Promise<EmptyResponse> DisbandGroup(long group)
		{
			return Requester.Request<EmptyResponse>(
			   Method.DELETE,
			   String.Format("/object/groups/{0}", group)
			);
		}

		public Promise<GroupMembershipResponse> LeaveGroup(long group)
		{
			return Requester.Request<GroupMembershipResponse>(
			   Method.DELETE,
			   String.Format("/object/group-users/{0}/join", Ctx.UserId),
			   new GroupMembershipRequest(group)
			);
		}

		public Promise<GroupMembershipResponse> JoinGroup(long group)
		{
			return Requester.Request<GroupMembershipResponse>(
			   Method.POST,
			   String.Format("/object/group-users/{0}/join", Ctx.UserId),
			   new GroupMembershipRequest(group)
			);
		}

		public Promise<EmptyResponse> Petition(long group)
		{
			return Requester.Request<EmptyResponse>(
			   Method.POST,
			   String.Format("/object/groups/{0}/petition", group),
			   ""
			);
		}

		public Promise<GroupSearchResponse> GetRecommendations()
		{
			return Requester.Request<GroupSearchResponse>(
			   Method.GET,
			   String.Format("/object/group-users/{0}/recommended", Ctx.UserId)
			);
		}

		public Promise<GroupSearchResponse> Search(
		   string name = null,
		   List<string> enrollmentTypes = null,
		   bool? hasSlots = null,
		   long? scoreMin = null,
		   long? scoreMax = null,
		   string sortField = null,
		   int? sortValue = null,
		   int? offset = null,
		   int? limit = null
		)
		{
			string args = "";

			if (!string.IsNullOrEmpty(name)) { args = AddQuery(args, "name", name); }
			if (offset.HasValue) { args = AddQuery(args, "offset", offset.ToString()); }
			if (limit.HasValue) { args = AddQuery(args, "limit", limit.ToString()); }
			if (enrollmentTypes != null) { args = AddQuery(args, "enrollmentTypes", string.Join(",", enrollmentTypes.ToArray())); }
			if (hasSlots.HasValue) { args = AddQuery(args, "hasSlots", hasSlots.Value.ToString()); }
			if (scoreMin.HasValue) { args = AddQuery(args, "scoreMin", scoreMin.Value.ToString()); }
			if (scoreMax.HasValue) { args = AddQuery(args, "scoreMax", scoreMax.Value.ToString()); }
			if (!string.IsNullOrEmpty(sortField)) { args = AddQuery(args, "sortField", sortField); }
			if (sortValue.HasValue) { args = AddQuery(args, "sortValue", sortValue.Value.ToString()); }

			return Requester.Request<GroupSearchResponse>(
			   Method.GET,
			   String.Format("/object/group-users/{0}/search?{1}", Ctx.UserId, args)
			);
		}

		public Promise<GroupCreateResponse> CreateGroup(GroupCreateRequest request)
		{
			using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
			{
				var dict = new ArrayDict();
				if (!string.IsNullOrEmpty(request.tag))
				{
					dict.Add("tag", request.tag);
				}

				dict.Add("enrollmentType", request.enrollmentType);
				dict.Add("requirement", request.requirement);
				dict.Add("maxSize", request.maxSize);
				dict.Add("name", request.name);

				var json = Json.Serialize(dict, pooledBuilder.Builder);
				return Requester.Request<GroupCreateResponse>(Method.POST, $"/object/group-users/{Ctx.UserId}/group", json);
			}
		}

		public Promise<AvailabilityResponse> CheckAvailability(string name, string tag)
		{
			string query = "";
			if (!string.IsNullOrEmpty(name))
			{
				query += "name=" + name;
			}
			if (!string.IsNullOrEmpty(tag))
			{
				if (name != null) { query += "&"; }
				query += "tag=" + tag;
			}
			return Requester.Request<AvailabilityResponse>(
			   Method.GET,
			   String.Format("/object/group-users/{0}/availability?{1}", Ctx.UserId, query)
			);
		}

		public Promise<EmptyResponse> SetGroupProps(long groupId, GroupUpdateProperties props)
		{
			using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
			{
				var dict = new ArrayDict();

				if (!string.IsNullOrEmpty(props.name))
				{
					dict.Add("name", props.name);
				}

				if (!string.IsNullOrEmpty(props.tag))
				{
					dict.Add("tag", props.tag);
				}

				if (!string.IsNullOrEmpty(props.enrollmentType))
				{
					dict.Add("enrollmentType", props.enrollmentType);
				}

				if (props.motd != null)
				{
					dict.Add("motd", props.motd);
				}

				if (props.slogan != null)
				{
					dict.Add("slogan", props.slogan);
				}

				if (props.clientData != null)
				{
					dict.Add("clientData", props.clientData);
				}

				if (props.requirement.HasValue)
				{
					dict.Add("requirement", props.requirement.Value);
				}

				var json = Json.Serialize(dict, pooledBuilder.Builder);
				return Requester.Request<EmptyResponse>(
				   Method.PUT,
				   $"/object/groups/{groupId}",
				   json
				);
			}
		}

		public Promise<GroupMembershipResponse> Kick(long group, long gamerTag)
		{
			return Requester.Request<GroupMembershipResponse>(
			   Method.DELETE,
			   String.Format("/object/groups/{0}/member", group),
			   new KickRequest(gamerTag)
			);
		}

		public Promise<EmptyResponse> SetRole(long group, long gamerTag, string role)
		{
			return Requester.Request<EmptyResponse>(
			   Method.PUT,
			   String.Format("/object/groups/{0}/role", group),
			   new RoleChangeRequest(gamerTag, role)
			);
		}

		public Promise<EmptyResponse> MakeDonationRequest(long group, Currency currency)
		{
			return Requester.Request<EmptyResponse>(
			   Method.POST,
			   $"/object/groups/{group}/donations",
			   new CreateDonationRequest(currency)
			);
		}

		public Promise<EmptyResponse> Donate(long group, long recipientId, long amount, bool autoClaim = true)
		{
			return Requester.Request<EmptyResponse>(
			   Method.PUT,
			   $"/object/groups/{group}/donations",
			   new MakeDonationRequest(recipientId, amount, autoClaim)
			);
		}

		public Promise<EmptyResponse> ClaimDonations(long group)
		{
			return Requester.Request<EmptyResponse>(
			   Method.PUT,
			   $"/object/groups/{group}/donations/claim"
			);
		}

		public string AddQuery(string query, string key, string value)
		{
			if (query.Length == 0)
			{
				return key + "=" + value;
			}
			else
			{
				return query + "&" + key + "=" + value;
			}
		}

		public virtual Promise<GroupsView> GetCurrent(string scope = "")
		{
			throw new NotImplementedException();
		}
	}

	[Serializable]
	public class GroupUser
	{
		/// <summary>
		/// The gamertag of this player
		/// </summary>
		public long gamerTag;

		/// <summary>
		/// A collection of <see cref="GroupMembership"/> memberships the player belongs to.
		/// </summary>
		public GroupMemberships member;

		/// <summary>
		/// The timestamp this structure was updated from the Beamable Cloud.
		/// The number of milliseconds from 1970-01-01T00:00:00Z.
		/// </summary>
		public long updated;
	}

	[Serializable]
	public class GroupMemberships
	{
		public List<GroupMembership> guild;
	}

	[Serializable]
	public class GroupMembership
	{
		/// <summary>
		/// The group id
		/// </summary>
		public long id;

		/// <summary>
		/// A set of group IDs that are sub groups
		/// </summary>
		public List<long> subGroups;

		/// <summary>
		/// The timestamp the player joined this group
		/// </summary>
		public long joined;
	}

	[Serializable]
	public class GroupsView
	{
		public List<GroupView> Groups;

		public void Update(GroupUser user, IEnumerable<Group> groups)
		{
			Groups = groups.Select(group =>
			{
				var membership = user.member.guild.Find(m => m.id == group.id);
				return new GroupView
				{
					Group = group,
					Joined = membership.joined
				};
			}).ToList();
		}
	}

	[Serializable]
	public class GroupView
	{
		public Group Group;
		public long Joined;
	}

	[Serializable]
	public class Group
	{
		/// <summary>
		/// The group id.
		/// </summary>
		public long id;

		/// <summary>
		/// The name of the group, which must be unique to the group, and at least 3 characters long.
		/// The name can be updated by the group Leader with the <see cref="IGroupsApi.SetGroupProps"/> method.
		/// </summary>
		public string name;

		/// <summary>
		/// An optional group tag. If the group has a tag, it must be unique. If the group does not have a tag, this field will be null.
		/// A tag must be exactly 3 characters.
		/// The tag can be updated by the group Leader with the <see cref="IGroupsApi.SetGroupProps"/> method.
		/// </summary>
		public string tag;

		/// <summary>
		/// A slogan is a piece of flavor text to give the group some character. Players will expect the slogan to remain fairly constant.
		/// The slogan can be updated by the group Leader with the <see cref="IGroupsApi.SetGroupProps"/> method.
		/// </summary>
		public string slogan;

		/// <summary>
		/// The Message Of The Day is a piece of flavor text that can be used to inspire players. Players will expect the motd to change often.
		/// The motd can be updated by the group Leader with the <see cref="IGroupsApi.SetGroupProps"/> method.
		/// </summary>
		public string motd;

		/// <summary>
		/// Indicates if the group is accepting new members.
		/// Valid values include "open", "closed", or "restricted".
		/// The enrollment can be updated by the group Leader with the <see cref="IGroupsApi.SetGroupProps"/> method.
		/// </summary>
		public string enrollmentType;

		/// <summary>
		/// This will be removed in a future version of Beamable. Please do not use.
		/// </summary>
		[Obsolete]
		public long requirement;

		/// <summary>
		/// The maximum number of players that can be in the group.
		/// Once the limit is reached, no new members will be allowed to join until existing members leave.
		/// </summary>
		public int maxSize;

		/// <summary>
		/// A list of <see cref="Member"/>s in the group from the perspective of the current player.
		/// The <see cref="Member.canDemote"/>, <see cref="Member.canPromote"/>, and <see cref="Member.canKick"/> fields
		/// indicate if the current player can perform the associated actions on the given member.
		/// </summary>
		public List<Member> members;

		/// <summary>
		/// A list of <see cref="SubGroup"/>s associated with the group.
		/// </summary>
		public List<SubGroup> subGroups;

		/// <summary>
		/// A general purpose string that can be used to fulfil developer needs.
		/// The clientData can be updated by the group Leader with the <see cref="IGroupsApi.SetGroupProps"/> method.
		/// </summary>
		public string clientData;

		/// <summary>
		/// A timestamp representing when the group was first created.
		/// The number of milliseconds from 1970-01-01T00:00:00Z.
		/// </summary>
		public long created;

		/// <summary>
		/// The number of members that can join before the group is full.
		/// This is always equal to the <see cref="maxSize"/> minus the current group size.
		/// </summary>
		public int freeSlots;

		/// <summary>
		/// true if the current player has the ability to disband the group.
		/// Only the group Leader has the ability to disband a group.
		/// A group can be disbanded with the <see cref="IGroupsApi.DisbandGroup"/> method.
		/// </summary>
		public bool canDisband;

		/// <summary>
		/// true if the current player has the ability to update the group's <see cref="enrollmentType"/>.
		/// Only the group Leader has the ability to update the <see cref="enrollmentType"/>.
		/// A group's <see cref="enrollmentType"/> can be updated with the <see cref="IGroupsApi.SetGroupProps"/> method.
		/// </summary>
		public bool canUpdateEnrollment;

		/// <summary>
		/// true if the current player has the ability to update the group's <see cref="motd"/>.
		/// Only the group Leader has the ability to update the <see cref="motd"/>.
		/// A group's <see cref="motd"/> can be updated with the <see cref="IGroupsApi.SetGroupProps"/> method.
		/// </summary>
		public bool canUpdateMOTD;

		/// <summary>
		/// true if the current player has the ability to update the group's <see cref="slogan"/>.
		/// Only the group Leader has the ability to update the <see cref="slogan"/>.
		/// A group's <see cref="slogan"/> can be updated with the <see cref="IGroupsApi.SetGroupProps"/> method.
		/// </summary>
		public bool canUpdateSlogan;

		/// <summary>
		/// A set of <see cref="DonationRequest"/>s that are currently pending in the group.
		/// <see cref="DonationRequest"/>s allow group members to ask for and receive currency from other group members.
		/// </summary>
		public List<DonationRequest> donations;
	}

	/// <summary>
	/// This structure defines a group member from the viewpoint of the current player's perspective.
	/// </summary>
	[Serializable]
	public class Member
	{
		/// <summary>
		/// The player gamertag
		/// </summary>
		public long gamerTag;

		/// <summary>
		/// Will be "leader", "officer", or null.
		/// The role indicates what permissions the player has in the group.
		/// Leaders can do anything. The player who created the group is the Leader, and there can only be 1 Leader per group.
		/// Officers are allowed to kick and promote regular group members.
		/// </summary>
		public string role;

		/// <summary>
		/// true if the current player has the ability to kick this <see cref="gamerTag"/> from the group.
		/// </summary>
		public bool canKick;

		/// <summary>
		/// true if the current player has the ability to promote this <see cref="gamerTag"/>.
		/// A regular user can be promoted to Officer by any existing Officer or the group Leader.
		/// There can only be 1 group Leader, so existing Officers cannot be promoted again.
		/// </summary>
		public bool canPromote;

		/// <summary>
		/// true if the current player has the ability to demote this <see cref="gamerTag"/>.
		/// Only Officers can be demoted. Only the group Leader can demote Officers.
		/// </summary>
		public bool canDemote;
	}

	[Serializable]
	public class SubGroup
	{
		/// <summary>
		/// The name of the sub group
		/// </summary>
		public string name;

		/// <summary>
		/// This will be removed in a future version of Beamable. Please do not use.
		/// </summary>
		public long requirement;

		/// <inheritdoc cref="Group.members"/>
		public List<Member> members;
	}

	[Serializable]
	public class GroupMembershipRequest
	{
		public long group;

		public GroupMembershipRequest(long group)
		{
			this.group = group;
		}
	}

	[Serializable]
	public class GroupMembershipResponse
	{
		public bool member;
	}

	[Serializable]
	public class GroupCreateRequest
	{
		/// <summary>
		/// A unique name for the group. This must be at least 3 characters.
		/// This will correlate to the resulting group's <see cref="Group.name"/>.
		/// </summary>
		public string name;

		/// <summary>
		/// An optional 3 character tag for the group.
		/// This will correlate to the resulting group's <see cref="Group.tag"/>.
		/// </summary>
		public string tag;

		/// <summary>
		/// The enrollment type must be either "open" or "closed".
		/// This will correlate to the resulting group's <see cref="Group.enrollmentType"/>.
		/// </summary>
		public string enrollmentType;

		/// <summary>
		/// This will be removed in a future version of Beamable. Please do not use this. Always leave it as 0.
		/// </summary>
		public long requirement;

		/// <summary>
		/// The maximum number of members that will be allowed to exist in the group at once.
		/// This will correlate to the resulting group's <see cref="Group.maxSize"/>.
		/// <b>WARNING!</b> This field cannot be updated later.
		/// </summary>
		public int maxSize;

		public GroupCreateRequest(string name, string tag, string enrollmentType, long requirement, int maxSize)
		{
			this.name = name;
			this.tag = tag;
			this.enrollmentType = enrollmentType;
			this.requirement = requirement;
			this.maxSize = maxSize;
		}
	}

	[Serializable]
	public class GroupCreateResponse
	{
		public GroupMetaData group;
	}

	[Serializable]
	public class GroupMetaData
	{
		/// <summary>
		/// The id of the group
		/// </summary>
		public long id;

		/// <summary>
		/// the name of the group. This is required.
		/// </summary>
		public string name;

		/// <summary>
		/// The tag of the group. This is optional.
		/// </summary>
		public string tag;
	}

	[Serializable]
	public class GroupSearchResponse
	{
		public List<Group> groups;
	}

	[Serializable]
	public class AvailabilityResponse
	{
		/// <summary>
		/// true when the name is available
		/// </summary>
		public bool name;

		/// <summary>
		/// true when the tag is available
		/// </summary>
		public bool tag;
	}

	[Serializable]
	public class KickRequest
	{
		public long gamerTag;
		public KickRequest(long gamerTag)
		{
			this.gamerTag = gamerTag;
		}
	}

	[Serializable]
	public class RoleChangeRequest
	{
		public long gamerTag;
		public string role;
		public RoleChangeRequest(long gamerTag, string role)
		{
			this.gamerTag = gamerTag;
			this.role = role;
		}
	}

	[Serializable]
	public class GroupUpdateProperties
	{
		/// <summary>
		/// The new slogan for the group.
		/// This correlates to the <see cref="Group.slogan"/> field.
		/// If this field is null, the group slogan will not be updated.
		/// </summary>
		public string slogan;

		/// <summary>
		/// The new Message Of The Day for the group.
		/// This correlates to the <see cref="Group.motd"/> field.
		/// If this field is null, the group motd will not be updated.
		/// </summary>
		public string motd;

		/// <summary>
		/// The new enrollmentType for the group. Valid values include "open", or "closed".
		/// This correlates to the <see cref="Group.enrollmentType"/> field.
		/// If this field is null, the group enrollmentType will not be updated.
		/// </summary>
		public string enrollmentType;

		/// <summary>
		/// The new clientData for the group.
		/// This correlates to the <see cref="Group.clientData"/> field.
		/// If this field is null, the group clientData will not be updated.
		/// </summary>
		public string clientData;

		/// <summary>
		/// This field will be removed in a future version of Beamable. Please do not use.
		/// </summary>
		public long? requirement;

		/// <summary>
		/// The new name for the group. The name must be unique, and at least 3 characters long.
		/// The <see cref="IGroupsApi.CheckAvailability"/> method can be used to check if a group name is available.
		/// This correlates to the <see cref="Group.name"/> field.
		/// If this field is null, the group name will not be updated.
		/// </summary>
		public string name;

		/// <summary>
		/// The new tag for the group. The tag must be unique, and exactly 3 characters long.
		/// The <see cref="IGroupsApi.CheckAvailability"/> method can be used to check if a group tag is available.
		/// This correlates to the <see cref="Group.tag"/> field.
		/// If this field is null, the group tag will not be updated.
		/// </summary>
		public string tag;
	}

	[Serializable]
	public class CreateDonationRequest
	{
		public string currencyId;
		public long amount;

		public CreateDonationRequest(Currency currency)
		{
			currencyId = currency.id;
			amount = currency.amount;

		}
	}

	[Serializable]
	public class MakeDonationRequest
	{
		public long recipientId;
		public long amount;
		public bool autoClaim;

		public MakeDonationRequest(long recipientId, long amount, bool autoClaim = true)
		{
			this.recipientId = recipientId;
			this.amount = amount;
			this.autoClaim = autoClaim;
		}
	}

	[Serializable]
	public class DonationRequest
	{
		/// <summary>
		/// the gamertag of the player who is requesting the donation.
		/// </summary>
		public long playerId;

		/// <summary>
		/// Currency type and amount of the donation requested.
		/// </summary>
		public Currency currency;

		/// <summary>
		/// Time this particular request was made.
		/// The number of milliseconds from 1970-01-01T00:00:00Z.
		/// </summary>
		public long timeRequested;

		/// <summary>
		/// List of all the members who have donated toward the request.
		/// </summary>
		public List<DonationEntry> progress;
	}

	[Serializable]
	public class DonationEntry
	{
		/// <summary>
		/// The gamertag of the player who donated.
		/// </summary>
		public long playerId;

		/// <summary>
		/// The amount of currency that was given.
		/// </summary>
		public long amount;

		/// <summary>
		/// The timestamp that the donation was made.
		/// The number of milliseconds from 1970-01-01T00:00:00Z.
		/// </summary>
		public long time;
	}
}
