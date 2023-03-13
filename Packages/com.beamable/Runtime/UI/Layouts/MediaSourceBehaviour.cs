using Beamable.UI.Layouts;
using System.Collections.Generic;
using UnityEngine;

public class MediaSourceBehaviour : MonoBehaviour
{
	public string Name;
	public RectTransform Target;
	public MediaQueryObject Query;


	private bool _first = true, _lastOutput = false;
	private List<MediaQueryCallback> _callbacks = new List<MediaQueryCallback>();

	// Update is called once per frame
	void Update()
	{
		var curr = Calculate();
		if (!_first && curr == _lastOutput) return;

		_first = false;
		_lastOutput = curr;
		Broadcast(curr);
	}

	void Broadcast(bool output)
	{
		for (var i = _callbacks.Count - 1; i >= 0; i--)
		{
			_callbacks[i]?.Invoke(this, output);
		}
	}

	public void Subscribe(MediaQueryCallback callback)
	{
		_callbacks.Add(callback);
	}

	public void Unsubscribe(MediaQueryCallback callback)
	{
		_callbacks.Remove(callback);
	}

	public bool Calculate()
	{
		return Query.Calculate(Target);
	}

}
