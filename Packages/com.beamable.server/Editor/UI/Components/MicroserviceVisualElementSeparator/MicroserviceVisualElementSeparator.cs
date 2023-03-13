using System;
using System.Collections.Generic;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements.StyleSheets;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace Beamable.Editor.Microservice.UI.Components
{
	public class MicroserviceVisualElementSeparator : MicroserviceComponent
	{
		private class SeparatorHandle : MouseManipulator
		{
			private bool _isDragging;
			public Action<float> OnHandleMoved;

			public SeparatorHandle()
			{
				activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
			}

			protected override void RegisterCallbacksOnTarget()
			{
#if UNITY_2019_1_OR_NEWER
                target.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
                target.RegisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.TrickleDown);
                target.RegisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
#elif UNITY_2018
                this.target.RegisterCallback<MouseDownEvent>(new EventCallback<MouseDownEvent>(this.OnMouseDown),
                    TrickleDown.TrickleDown);
                this.target.RegisterCallback<MouseMoveEvent>(new EventCallback<MouseMoveEvent>(this.OnMouseMove),
                    TrickleDown.TrickleDown);
                this.target.RegisterCallback<MouseUpEvent>(new EventCallback<MouseUpEvent>(this.OnMouseUp),
                    TrickleDown.TrickleDown);
#endif
			}

			protected override void UnregisterCallbacksFromTarget()
			{
#if UNITY_2019_1_OR_NEWER
                target.UnregisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
                target.UnregisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.TrickleDown);
                target.UnregisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
#elif UNITY_2018
                this.target.UnregisterCallback<MouseDownEvent>(new EventCallback<MouseDownEvent>(this.OnMouseDown),
                    TrickleDown.TrickleDown);
                this.target.UnregisterCallback<MouseMoveEvent>(new EventCallback<MouseMoveEvent>(this.OnMouseMove),
                    TrickleDown.TrickleDown);
                this.target.UnregisterCallback<MouseUpEvent>(new EventCallback<MouseUpEvent>(this.OnMouseUp),
                    TrickleDown.TrickleDown);
#endif
			}

			private void OnMouseUp(MouseUpEvent e)
			{
				if (_isDragging && CanStopManipulation(e))
				{
					_isDragging = false;
					target.ReleaseMouse();
					e.StopPropagation();
				}
			}

			private void OnMouseDown(MouseDownEvent e)
			{
				if (CanStartManipulation(e))
				{
					MicroserviceVisualElementSeparator separator = target as MicroserviceVisualElementSeparator;
					separator.CaptureMouse();
					e.StopPropagation();
					_isDragging = true;
				}
			}

			private void OnMouseMove(MouseMoveEvent e)
			{
				if (_isDragging)
				{
					OnHandleMoved?.Invoke(e.mouseDelta.y);
					e.StopPropagation();
				}
			}
		}

		private Action<float> _onDragAction;

		public MicroserviceVisualElementSeparator() : base(nameof(MicroserviceVisualElementSeparator))
		{
			SeparatorHandle handle = new SeparatorHandle();
			handle.OnHandleMoved += value => { _onDragAction?.Invoke(value); };
			this.AddManipulator((IManipulator)handle);
		}

		public new class UxmlFactory : UxmlFactory<MicroserviceVisualElementSeparator, UxmlTraits>
		{
		}

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}
		}

		public void Setup(Action<float> onDragAction)
		{
			_onDragAction = onDragAction;
		}
	}
}
