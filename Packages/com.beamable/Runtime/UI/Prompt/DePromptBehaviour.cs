using Beamable.UI.Buttons;
using Beamable.UI.Prompt;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DePromptBehaviour : MonoBehaviour
{
	public DeButtonBehaviour Button;
	public TextMeshProUGUI Title;
	public TextMeshProUGUI Message;

	public UnityEvent OnClicked;

	public void Set(DePromptData data)
	{
		if (!string.IsNullOrEmpty(data.Title))
		{
			Title.text = data.Title;
		}

		if (!string.IsNullOrEmpty(data.Message))
		{
			Message.text = data.Message;

		}

		if (!string.IsNullOrEmpty(data.ButtonText))
		{
			Button.Text = data.ButtonText;
		}
	}

	public void Trigger()
	{
		OnClicked?.Invoke();
	}
}
