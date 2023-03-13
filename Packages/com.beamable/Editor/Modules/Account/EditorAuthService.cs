using Beamable.Api;
using Beamable.Api.Auth;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

namespace Beamable.Editor.Modules.Account
{
	public interface IEditorAuthApi : IAuthService
	{
		Promise<EditorUser> GetUserForEditor();
	}

	public class EditorAuthService : AuthService, IEditorAuthApi
	{
		public EditorAuthService(IBeamableRequester requester) : base(requester, new DefaultDeviceIdResolver(), new DefaultAuthSettings
		{
			PasswordResetCodeType = CodeType.PIN
		})
		{
			if (requester is PlatformRequester platformRequester)
				platformRequester.AuthService = this;
		}

		// This API call will only work if made by editor code.
		public Promise<EditorUser> GetUserForEditor()
		{
			return Requester.Request<EditorUser>(Method.GET, $"{ACCOUNT_URL}/admin/me", useCache: true);
		}
	}

	[System.Serializable]
	public class EditorUserRealmPermission
	{
		/// <summary>
		/// The role will either be tester, developer, or admin
		/// </summary>
		public string role;

		/// <summary>
		/// The pid specifies for which realm this permission applies. A user may be a tester on one realm, but an admin on another.
		/// </summary>
		public string projectId;
	}

	[System.Serializable]
	public class EditorUser : User
	{
		public const string ADMIN_ROLE = "admin";
		public const string DEVELOPER_ROLE = "developer";
		public const string TESTER_ROLE = "tester";

		/// <summary>
		/// The role will be either admin, developer or tester. This is the global role for the user for all realms. However, there
		/// may be realm specific overrides in the <see cref="roles"/> list.
		/// Use the <see cref="GetPermissionsForRealm"/> method to find the role for the given realm of interest.
		/// </summary>
		[Obsolete("Use " + nameof(GetPermissionsForRealm) + " method, or " + nameof(GlobalPermissions) + " property instead.")]
		public string roleString;

		/// <summary>
		/// The list of <see cref="EditorUserRealmPermission"/> describes the realm specific overrides for the users permissions.
		/// By default, in every realm, the user will have the role assigned in the  <see cref="roleString"/> field. However, this
		/// list may contain realm specific overrides that grant the user higher or lower privileges in the given realm.
		/// </summary>
		public List<EditorUserRealmPermission> roles = new List<EditorUserRealmPermission>();

		private UserPermissions _globalPermissions;

		/// <summary>
		/// The global <see cref="UserPermissions"/> contain the permissions for the user's global role.
		/// <b>The user's roles for the current realm may be different!</b> Please use the <see cref="GetPermissionsForRealm"/> method.
		/// </summary>
		public UserPermissions GlobalPermissions =>
#pragma warning disable CS0618
			_globalPermissions ?? (_globalPermissions = new UserPermissions(roleString));
#pragma warning restore CS0618

		[Obsolete("Please use the " + nameof(GetPermissionsForRealm) + " method instead.")]
		public bool IsAtLeastAdmin => GlobalPermissions.IsAtLeastAdmin;

		[Obsolete("Please use the " + nameof(GetPermissionsForRealm) + " method instead.")]
		public bool IsAtLeastDeveloper => GlobalPermissions.IsAtLeastDeveloper;

		[Obsolete("Please use the " + nameof(GetPermissionsForRealm) + " method instead.")]
		public bool IsAtLeastTester => GlobalPermissions.IsAtLeastTester;

		[Obsolete("Please use the " + nameof(GetPermissionsForRealm) + " method instead.")]
		public bool HasNoRole => !IsAtLeastTester;

		[Obsolete("Please use the " + nameof(GetPermissionsForRealm) + " method instead.")]
		public bool CanPushContent => IsAtLeastDeveloper;

		public EditorUser()
		{

		}

		public EditorUser(User user)
		{
			id = user.id;
			email = user.email;
			language = user.language;
			scopes = user.scopes;
			thirdPartyAppAssociations = user.thirdPartyAppAssociations;
			deviceIds = user.deviceIds;
		}

		/// <summary>
		/// Get the user's <see cref="UserPermissions"/> for the given realm.
		/// By default, a user's role will be equal to the <see cref="roleString"/> field. However, the contents of the <see cref="roles"/> list
		/// may contain realm specific overrides for the user's permissions.
		/// </summary>
		/// <param name="pid">Some realm pid.</param>
		/// <returns>The user's permissions for the current realm.</returns>
		public UserPermissions GetPermissionsForRealm(string pid)
		{
			if (string.IsNullOrEmpty(pid)) return GlobalPermissions;

			var realmOverride = roles?.FirstOrDefault(role => string.Equals(role.projectId, pid));
			if (realmOverride == null) return GlobalPermissions;
			return new UserPermissions(realmOverride.role);
		}

	}

	/// <summary>
	/// Describes the permissions for a user.
	/// </summary>
	[System.Serializable]
	public class UserPermissions
	{
		public string Role { get; }

		public UserPermissions(string role)
		{
			Role = role;
		}

		/// <summary>
		/// Returns true when the <see cref="Role"/> is an admin.
		/// </summary>
		public bool IsAtLeastAdmin => string.Equals(Role, EditorUser.ADMIN_ROLE);

		/// <summary>
		/// Returns true when the <see cref="Role"/> is developer or admin.
		/// </summary>
		public bool IsAtLeastDeveloper => IsAtLeastAdmin || string.Equals(Role, EditorUser.DEVELOPER_ROLE);

		/// <summary>
		/// Returns true when the <see cref="Role"/> is tester, developer, or admin.
		/// </summary>
		public bool IsAtLeastTester => IsAtLeastDeveloper || string.Equals(Role, EditorUser.TESTER_ROLE);

		/// <summary>
		/// Returns true when the <see cref="Role"/> is not tester, developer, or admin.
		/// </summary>
		public bool HasNoRole => !IsAtLeastTester;

		/// <summary>
		/// Returns true when the user has the permission to publish Content.
		/// </summary>
		public bool CanPushContent => IsAtLeastDeveloper;

		/// <summary>
		/// Returns true when the user has the permission to publish Microservice and Microstorages.
		/// </summary>
		public bool CanPublishMicroservices => IsAtLeastDeveloper;
	}
}
