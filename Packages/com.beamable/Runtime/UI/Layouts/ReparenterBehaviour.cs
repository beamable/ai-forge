using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Beamable.UI.Layouts
{
	public class ReparenterBehaviour : MediaQueryBehaviour
	{
		public Transform Origin, Destination;

		public bool Output { get; private set; }
		private Transform _parent;

		public Transform CurrentParent => _parent ?? Origin;

		public List<GameObject> EnabledOnlyAtOrigin, EnabledOnlyAtDest;

		public void MoveToOrigin()
		{
			MoveChildren(Destination, Origin);
			SetEnabledList(EnabledOnlyAtOrigin, true);
			SetEnabledList(EnabledOnlyAtDest, false);
		}

		public void MoveToDestination()
		{
			MoveChildren(Origin, Destination);
			SetEnabledList(EnabledOnlyAtOrigin, false);
			SetEnabledList(EnabledOnlyAtDest, true);
		}

		private void SetEnabledList(List<GameObject> gobs, bool enable)
		{
			foreach (var gob in gobs)
			{
				if (!gob) continue;
				gob.SetActive(enable);
			}
		}

		public void Move(bool toDestination)
		{
			if (toDestination)
			{
				_parent = Destination;
				MoveToDestination();
			}
			else
			{
				_parent = Origin;
				MoveToOrigin();
			}
		}

		protected override void OnMediaQueryChange(MediaSourceBehaviour query, bool output)
		{
			Output = output;
			Move(output);
			base.OnMediaQueryChange(query, output);
		}

		void MoveChildren(Transform from, Transform to)
		{
			var children = new List<Transform>();
			for (var i = 0; i < from.childCount; i++)
			{
				children.Add(from.GetChild(i));
			}

			foreach (var child in children)
			{
				child.SetParent(to, false);
			}

			from.gameObject.SetActive(false);
			to.gameObject.SetActive(true);

			if (to is RectTransform rect)
			{
				LayoutRebuilder.MarkLayoutForRebuild(rect);
			}
		}

	}
}
