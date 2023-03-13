using JetBrains.Annotations;
using UnityEngine.Events;

namespace Beamable.UI.Prompt
{
	[System.Serializable]
	public class DePromptUnityEvent : UnityEvent<DePromptData> { }

	[System.Serializable]
	public class DePromptData
	{
		[CanBeNull] public string Title, Message, ButtonText;
	}
}
