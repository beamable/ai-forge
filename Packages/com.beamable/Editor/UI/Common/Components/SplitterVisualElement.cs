using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
namespace Beamable.Editor.UI.Components
{
// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

    internal class SplitterVisualElement : ImmediateModeElement
    {
        const int kDefaultSplitSize = 10;
        public int splitSize = kDefaultSplitSize;
        public Action<List<float>> OnFlexChanged;

        public void EmitFlexValues()
        {
           var flexValues = Children().Select(child => child.style.flexGrow.value).ToList();

           OnFlexChanged?.Invoke(flexValues);
        }

        public new class UxmlFactory : UxmlFactory<SplitterVisualElement, UxmlTraits> {}

        public new class UxmlTraits : VisualElement.UxmlTraits {}

        private class SplitManipulator : MouseManipulator
        {
            private int m_ActiveVisualElementIndex = -1;
            private int m_NextVisualElementIndex = -1;
            public Action<List<float>> OnFlexChanged;

            private List<VisualElement> m_AffectedElements;

            bool m_Active;

            public SplitManipulator()
            {
                activators.Add(new ManipulatorActivationFilter {button = MouseButton.LeftMouse});
            }

            protected override void RegisterCallbacksOnTarget()
            {
                target.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
                target.RegisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.TrickleDown);
                target.RegisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                target.UnregisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
                target.UnregisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.TrickleDown);
                target.UnregisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
            }

            protected void OnMouseDown(MouseDownEvent e)
            {
                if (CanStartManipulation(e))
                {
                   SplitterVisualElement visualSplitter = target as SplitterVisualElement;
                    FlexDirection flexDirection = visualSplitter.resolvedStyle.flexDirection;

                    if (m_AffectedElements != null)
                    {
                       m_AffectedElements.Clear();
                    }
                    m_AffectedElements = visualSplitter.GetAffectedVisualElements();

                    for (int i = 0; i < m_AffectedElements.Count - 1; ++i)
                    {
                        VisualElement visualElement = m_AffectedElements[i];

                        Rect splitterRect = visualSplitter.GetSplitterRect(visualElement);

                        if (splitterRect.Contains(e.localMousePosition))
                        {
                            bool isReverse = flexDirection == FlexDirection.RowReverse || flexDirection == FlexDirection.ColumnReverse;

                            if (isReverse)
                            {
                                m_ActiveVisualElementIndex = i + 1;
                                m_NextVisualElementIndex = i;
                            }
                            else
                            {
                                m_ActiveVisualElementIndex = i;
                                m_NextVisualElementIndex = i + 1;
                            }

                            m_Active = true;
                            target.CaptureMouse();
                            e.StopPropagation();
                        }
                    }
                }
            }

            protected void OnMouseMove(MouseMoveEvent e)
            {
                if (m_Active)
                {
                    // These calculations should only work if flex-basis is auto.
                    // However, Yoga implementation of flex-basis 0 is broken and behaves much like
                    // flex-basis auto, so it currently works with flex-basis 0 too.

                    SplitterVisualElement visualSplitter = target as SplitterVisualElement;
                    VisualElement visualElement = m_AffectedElements[m_ActiveVisualElementIndex];
                    VisualElement nextVisualElement = m_AffectedElements[m_NextVisualElementIndex];

                    FlexDirection flexDirection = visualSplitter.resolvedStyle.flexDirection;
                    bool isVertical = flexDirection == FlexDirection.Column || flexDirection == FlexDirection.ColumnReverse;

                    float relativeMousePosition;
                    if (isVertical)
                    {
                        float availableHeight = visualElement.layout.height + nextVisualElement.layout.height;
                        float minHeight = Mathf.Max(
                            visualElement.resolvedStyle.minHeight.value, // make sure it's at least min
                            availableHeight - (nextVisualElement.resolvedStyle.maxHeight.value <= 0f ? availableHeight : nextVisualElement.resolvedStyle.maxHeight.value) // also make sure that it's more then all size - next element max size
                            );
                        float maxHeight = Mathf.Min(
                            visualElement.resolvedStyle.maxHeight.value <= 0 ? availableHeight : visualElement.resolvedStyle.maxHeight.value, // make sure it's not more then max
                            availableHeight - nextVisualElement.resolvedStyle.minHeight.value // also make sure it's leaving place for next element with min size
                            );
                        

                        relativeMousePosition =
                            (Mathf.Clamp(e.mousePosition.y - visualElement.worldBound.yMin - 2f, minHeight, maxHeight) - minHeight) // -2f is here to make splitter a bit above the cursor point
                            / (maxHeight - minHeight);
                    }
                    else
                    {
                        float availableWidth = visualElement.layout.width + nextVisualElement.layout.width;
                        float minWidth = Mathf.Max(
                            visualElement.resolvedStyle.minWidth.value, // make sure it's at least min
                            availableWidth - (nextVisualElement.resolvedStyle.maxWidth.value <= 0f ? availableWidth : nextVisualElement.resolvedStyle.maxWidth.value) // also make sure that it's more then all size - next element max size
                        );
                        float maxWidth = Mathf.Min(
                            visualElement.resolvedStyle.maxWidth.value <= 0 ? availableWidth : visualElement.resolvedStyle.maxWidth.value, // make sure it's not more then max
                            availableWidth - nextVisualElement.resolvedStyle.minWidth.value // also make sure it's leaving place for next element with min size
                        );
                        
                        relativeMousePosition =
                            (Mathf.Clamp(e.mousePosition.x - visualElement.worldBound.xMin, minWidth, maxWidth) - minWidth)
                            / (maxWidth - minWidth);
                    }

                    relativeMousePosition = Math.Max(0.0f, Math.Min(0.999f, relativeMousePosition));

                    float totalFlex = visualElement.resolvedStyle.flexGrow + nextVisualElement.resolvedStyle.flexGrow;
                    visualElement.style.flexGrow = relativeMousePosition * totalFlex;
                    nextVisualElement.style.flexGrow = (1.0f - relativeMousePosition) * totalFlex;

                    e.StopPropagation();
                    EmitFlexValues();
                }
            }

            protected void OnMouseUp(MouseUpEvent e)
            {
                if (m_Active && CanStopManipulation(e))
                {
                    m_Active = false;
                    target.ReleaseMouse();
                    e.StopPropagation();

                    m_ActiveVisualElementIndex = -1;
                    m_NextVisualElementIndex = -1;
                }
            }
            protected void EmitFlexValues()
            {
               var flexValues = target.Children().Select(child => child.style.flexGrow.value).ToList();

               OnFlexChanged?.Invoke(flexValues);
            }

        }

        public static readonly string ussClassName = "unity-visual-splitter";

        public SplitterVisualElement()
        {
            AddToClassList(ussClassName);
            var manip = new SplitManipulator();
            manip.OnFlexChanged += vals => { OnFlexChanged?.Invoke(vals); };
            this.AddManipulator(manip);
        }

        public List<VisualElement> GetAffectedVisualElements()
        {
            List<VisualElement> elements = new List<VisualElement>();
            var count = hierarchy.childCount;
            for (int i = 0; i < count; ++i)
            {
                VisualElement element = hierarchy[i];
                if (element.resolvedStyle.position == Position.Relative)
                    elements.Add(element);
            }

            return elements;
        }

        protected override void ImmediateRepaint()
        {
            UpdateCursorRects();
        }

        void UpdateCursorRects()
        {
            var count = hierarchy.childCount;
            for (int i = 0; i < count - 1; ++i)
            {
                VisualElement visualElement = hierarchy[i];
                bool isVertical = resolvedStyle.flexDirection == FlexDirection.Column || resolvedStyle.flexDirection == FlexDirection.ColumnReverse;

                EditorGUIUtility.AddCursorRect(GetSplitterRect(visualElement), isVertical ? MouseCursor.ResizeVertical : MouseCursor.SplitResizeLeftRight);
            }
        }

        public Rect GetSplitterRect(VisualElement visualElement)
        {
            Rect rect = visualElement.layout;
            if (resolvedStyle.flexDirection == FlexDirection.Row)
            {
                rect.xMin = visualElement.layout.xMax - splitSize * 0.5f;
                rect.xMax = visualElement.layout.xMax + splitSize * 0.5f;
            }
            else if (resolvedStyle.flexDirection == FlexDirection.RowReverse)
            {
                rect.xMin = visualElement.layout.xMin - splitSize * 0.5f;
                rect.xMax = visualElement.layout.xMin + splitSize * 0.5f;
            }
            else if (resolvedStyle.flexDirection == FlexDirection.Column)
            {
                rect.yMin = visualElement.layout.yMax - splitSize * 0.5f;
                rect.yMax = visualElement.layout.yMax + splitSize * 0.5f;
            }
            else if (resolvedStyle.flexDirection == FlexDirection.ColumnReverse)
            {
                rect.yMin = visualElement.layout.yMin - splitSize * 0.5f;
                rect.yMax = visualElement.layout.yMin + splitSize * 0.5f;
            }

            return rect;
        }
    }
}

#endif

#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements.StyleSheets;

namespace Beamable.Editor.UI.Components
{

  class SplitterVisualElement : VisualElement // XXX: Taken from Unity 2018.
  {
    public int splitSize = 30;
    public Action<List<float>> OnFlexChanged;

    public SplitterVisualElement()
    {
       var manipulator = new SplitterVisualElement.SplitManipulator();
       manipulator.OnFlexChanged += vals =>
       {
          OnFlexChanged?.Invoke(vals);
       };
      this.AddManipulator((IManipulator) manipulator);
    }

    public void EmitFlexValues()
    {
       var flexValues = Children().Select(child => child.style.flexGrow.value).ToList();

       OnFlexChanged?.Invoke(flexValues);
    }


    public List<VisualElement> GetAffectedVisualElements()
    {
      List<VisualElement> visualElementList = new List<VisualElement>(); // XXX: This used to be a pool.
      for (int index = 0; index < this.shadow.childCount; ++index)
      {
        VisualElement visualElement = this.shadow[index];
        if ((PositionType) visualElement.style.positionType == PositionType.Relative)
          visualElementList.Add(visualElement);
      }

      return visualElementList;
    }

    protected override void DoRepaint(IStylePainter painter)
    {
      for (int index = 0; index < this.shadow.childCount - 1; ++index)
        EditorGUIUtility.AddCursorRect(this.GetSplitterRect(this.shadow[index]),
          (FlexDirection) this.style.flexDirection != FlexDirection.Column &&
          (FlexDirection) this.style.flexDirection != FlexDirection.ColumnReverse
            ? MouseCursor.SplitResizeLeftRight
            : MouseCursor.ResizeVertical);
    }

    public Rect GetSplitterRect(VisualElement visualElement)
    {
      Rect layout = visualElement.layout;
      if ((FlexDirection) this.style.flexDirection == FlexDirection.Row)
      {
        layout.xMin = visualElement.layout.xMax - (float) this.splitSize * 0.5f;
        layout.xMax = visualElement.layout.xMax + (float) this.splitSize * 0.5f;
      }
      else if ((FlexDirection) this.style.flexDirection == FlexDirection.RowReverse)
      {
        layout.xMin = visualElement.layout.xMin - (float) this.splitSize * 0.5f;
        layout.xMax = visualElement.layout.xMin + (float) this.splitSize * 0.5f;
      }
      else if ((FlexDirection) this.style.flexDirection == FlexDirection.Column)
      {
        layout.yMin = visualElement.layout.yMax - (float) this.splitSize * 0.5f;
        layout.yMax = visualElement.layout.yMax + (float) this.splitSize * 0.5f;
      }
      else if ((FlexDirection) this.style.flexDirection == FlexDirection.ColumnReverse)
      {
        layout.yMin = visualElement.layout.yMin - (float) this.splitSize * 0.5f;
        layout.yMax = visualElement.layout.yMin + (float) this.splitSize * 0.5f;
      }

      return layout;
    }

    public new class UxmlFactory : UnityEngine.Experimental.UIElements.UxmlFactory<SplitterVisualElement,
      SplitterVisualElement.UxmlTraits>
    {
    }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
    }

    private class SplitManipulator : MouseManipulator
    {
      private int m_ActiveVisualElementIndex = -1;
      private int m_NextVisualElementIndex = -1;
      private List<VisualElement> m_AffectedElements;
      private bool m_Active;

      public Action<List<float>> OnFlexChanged;

      public SplitManipulator()
      {
        this.activators.Add(new ManipulatorActivationFilter()
        {
          button = MouseButton.LeftMouse
        });
      }

      protected override void RegisterCallbacksOnTarget()
      {
        this.target.RegisterCallback<MouseDownEvent>(new EventCallback<MouseDownEvent>(this.OnMouseDown),
          TrickleDown.TrickleDown);
        this.target.RegisterCallback<MouseMoveEvent>(new EventCallback<MouseMoveEvent>(this.OnMouseMove),
          TrickleDown.TrickleDown);
        this.target.RegisterCallback<MouseUpEvent>(new EventCallback<MouseUpEvent>(this.OnMouseUp),
          TrickleDown.TrickleDown);
      }

      protected override void UnregisterCallbacksFromTarget()
      {
        this.target.UnregisterCallback<MouseDownEvent>(new EventCallback<MouseDownEvent>(this.OnMouseDown),
          TrickleDown.TrickleDown);
        this.target.UnregisterCallback<MouseMoveEvent>(new EventCallback<MouseMoveEvent>(this.OnMouseMove),
          TrickleDown.TrickleDown);
        this.target.UnregisterCallback<MouseUpEvent>(new EventCallback<MouseUpEvent>(this.OnMouseUp),
          TrickleDown.TrickleDown);
      }

      protected void OnMouseDown(MouseDownEvent e)
      {
        if (!this.CanStartManipulation((IMouseEvent) e))
          return;
        SplitterVisualElement target = this.target as SplitterVisualElement;
        FlexDirection flexDirection = (FlexDirection) target.style.flexDirection;
        if (this.m_AffectedElements != null)
          this.m_AffectedElements.Clear(); // XXX: This used to Release() a pool
        this.m_AffectedElements = target.GetAffectedVisualElements();
        for (int index = 0; index < this.m_AffectedElements.Count - 1; ++index)
        {
          VisualElement affectedElement = this.m_AffectedElements[index];
          if (target.GetSplitterRect(affectedElement).Contains(e.localMousePosition))
          {
            if (flexDirection == FlexDirection.RowReverse || flexDirection == FlexDirection.ColumnReverse)
            {
              this.m_ActiveVisualElementIndex = index + 1;
              this.m_NextVisualElementIndex = index;
            }
            else
            {
              this.m_ActiveVisualElementIndex = index;
              this.m_NextVisualElementIndex = index + 1;
            }

            this.m_Active = true;
            this.target.CaptureMouse();
            e.StopPropagation();
          }
        }
      }

      protected void OnMouseMove(MouseMoveEvent e)
      {
        if (!this.m_Active)
          return;
        SplitterVisualElement target = this.target as SplitterVisualElement;
        VisualElement affectedElement1 = this.m_AffectedElements[this.m_ActiveVisualElementIndex];
        VisualElement affectedElement2 = this.m_AffectedElements[this.m_NextVisualElementIndex];
        FlexDirection flexDirection = (FlexDirection) target.style.flexDirection;
        float num1 = Math.Max(0.0f,
          Math.Min(1f,
            flexDirection != FlexDirection.Column && flexDirection != FlexDirection.ColumnReverse
              ? (float) (((double) e.localMousePosition.x - (double) affectedElement1.layout.xMin -
                          (double) (float) affectedElement1.style.minWidth) /
                         ((double) affectedElement1.layout.width + (double) affectedElement2.layout.width -
                          (double) (float) affectedElement1.style.minWidth -
                          (double) (float) affectedElement2.style.minWidth))
              : (float) (((double) e.localMousePosition.y - (double) affectedElement1.layout.yMin -
                          (double) (float) affectedElement1.style.minHeight) /
                         ((double) affectedElement1.layout.height + (double) affectedElement2.layout.height -
                          (double) (float) affectedElement1.style.minHeight -
                          (double) (float) affectedElement2.style.minHeight))));
        float num2 = (float) affectedElement1.style.flexGrow + (float) affectedElement2.style.flexGrow;
        affectedElement1.style.flexGrow = (StyleValue<float>) (num1 * num2);
        affectedElement2.style.flexGrow = (StyleValue<float>) ((1f - num1) * num2);
        e.StopPropagation();
        EmitFlexValues();
      }

      protected void EmitFlexValues()
      {
         var flexValues = target.Children().Select(child => child.style.flexGrow.value).ToList();

         OnFlexChanged?.Invoke(flexValues);
      }

      protected void OnMouseUp(MouseUpEvent e)
      {
        if (!this.m_Active || !this.CanStopManipulation((IMouseEvent) e))
          return;
        this.m_Active = false;
        this.target.ReleaseMouse();
        e.StopPropagation();
        this.m_ActiveVisualElementIndex = -1;
        this.m_NextVisualElementIndex = -1;
      }
    }
  }
}
#endif
