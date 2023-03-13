using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollDotsBehaviour : MonoBehaviour
{

	public ScrollRect Scroller;
	public Sprite DotSprite;
	public Sprite DotSpriteActive;
	public Color DotColor, DotColorActive;

	private List<Image> _dots = new List<Image>();

	// Start is called before the first frame update
	void Start()
	{
		Refresh();
		Scroller.onValueChanged.AddListener(HandleScroll);
	}

	// Update is called once per frame
	void Update()
	{
		if (GetScreenWidths() != _dots.Count)
		{
			Refresh();
		}
	}

	public int GetScreenWidths()
	{
		var contentWidth = Scroller.content.rect.width;
		var viewportWidth = Scroller.viewport.rect.width;
		var count = Mathf.CeilToInt(contentWidth / viewportWidth);

		return count == 1 ? 0 : count; // if there is only one page, don't bother.
	}

	public void Refresh()
	{
		// remove all children.
		for (var i = 0; i < transform.childCount; i++)
		{
			Destroy(transform.GetChild(i).gameObject);
		}
		_dots.Clear();

		// make a new dot per screen-width
		var screenWidths = GetScreenWidths();
		for (var i = 0; i < screenWidths; i++)
		{
			var dot = new GameObject("dot" + i, typeof(RectTransform));
			dot.transform.SetParent(transform);
			var layout = dot.AddComponent<LayoutElement>();
			layout.preferredHeight = 20;
			layout.preferredWidth = 20;
			var image = dot.AddComponent<Image>();
			image.sprite = DotSprite;
			image.color = DotColor;
			image.preserveAspect = true;
			_dots.Add(image);
		}

		HandleScroll(Vector2.zero);
	}

	void HandleScroll(Vector2 pos)
	{
		var screenWidths = GetScreenWidths();

		var x = Scroller.normalizedPosition.x;
		var page = (int)Math.Floor(x * screenWidths);
		page = Mathf.Clamp(page, 0, _dots.Count - 1);
		for (var i = 0; i < _dots.Count; i++)
		{
			var isActive = page == i;
			_dots[i].color = isActive ? DotColorActive : DotColor;
			_dots[i].sprite = isActive ? DotSpriteActive : DotSprite;
		}
	}
}
