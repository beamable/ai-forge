using Beamable.Common.Content;
using UnityEngine;

namespace Beamable.Editor.Content
{
	[System.Serializable]
	public class SerializedContentObject<TContent> : ScriptableObject
	   where TContent : ContentObject, new()
	{
		public TContent Content;
	}
}
