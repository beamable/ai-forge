using UnityEngine;
using UnityEngine.EventSystems;

namespace Beamable.UI.Scripts
{
	[ExecuteInEditMode]
	public class TransformOffsetBehaviour : UIBehaviour
	{
		public RectTransform Target;
		public Vector2 Offset;
		public Vector2 Scale;

		protected override void Start()
		{
			base.Start();
			ApplyOffset();

		}

		/// <summary>
		///   <para>See MonoBehaviour.OnEnable.</para>
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			ApplyOffset();
		}

		protected override void OnCanvasGroupChanged()
		{
			base.OnCanvasGroupChanged();
			ApplyOffset();

		}

		protected override void OnTransformParentChanged()
		{
			base.OnTransformParentChanged();
			ApplyOffset();

		}

		protected override void OnCanvasHierarchyChanged()
		{
			base.OnCanvasHierarchyChanged();
			ApplyOffset();
		}

		public void ApplyOffset()
		{
			if (Target == null) return;

			Target.anchoredPosition = Offset;
			Target.anchorMin = Vector2.up;
			Target.anchorMax = Vector2.up;
			Target.localScale = Scale;

			Target.ForceUpdateRectTransforms();
		}
	}
}
