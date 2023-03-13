using UnityEngine;
using UnityEngine.Serialization;
#pragma warning disable CS0618

namespace Beamable.Common.Content
{
	/// <summary>
	/// A set of permissions that specify how a player can interact with a resource
	/// </summary>
	[System.Serializable]
	[Agnostic]
	public class ClientPermissions
	{
		/// <summary>
		/// If true, then the player can make edits for this object pertaining to their own account. Even if this setting is true,
		/// a player cannot edit another player's settings.
		/// </summary>
		[Tooltip(ContentObject.WriteSelf1)]
		[FormerlySerializedAs("write_self")]
		[ContentField("write_self")]
		public bool writeSelf;
	}

	[System.Serializable]
	[Agnostic]
	public class OptionalClientPermissions : Optional<ClientPermissions> { }

}
