using Beamable.Coroutines;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrollToAlphaBehaviour : MonoBehaviour
{
	public RectTransform.Axis Axis;
	public CanvasGroup Group;
	public ScrollRect Scroller;
	public float Edge0, Edge1, Min = 0, Max = 1;
	public float ForceHideThreshold = .5f;

	// Start is called before the first frame update
	void Start()
	{
		Scroller.onValueChanged.AddListener(_ => HandleScroll());
		StartCoroutine(Init());
	}

	IEnumerator Init()
	{
		yield return Yielders.EndOfFrame;
		HandleScroll();
	}

	void HandleScroll()
	{
		// TODO: the fade happens in % space, which may not work for dynamically sized lists...
		var i = Axis == RectTransform.Axis.Horizontal ? Scroller.normalizedPosition.x : Scroller.normalizedPosition.y;
		var alpha = Smoothstep(Edge0, Edge1, i);
		alpha = Mathf.Clamp(alpha, Min, Max);


		if ((Scroller.content.rect.width - Scroller.viewport.rect.width) < ForceHideThreshold)
		{
			alpha = 0;
		}
		Group.alpha = alpha;
	}

	float Smoothstep(float edge0, float edge1, float x)
	{
		var t = Mathf.Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
		return t * t * (3.0f - 2.0f * t);
	}
}
