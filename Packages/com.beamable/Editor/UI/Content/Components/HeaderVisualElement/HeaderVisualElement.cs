using Beamable.Editor.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{
	public struct HeaderSizeChange
	{
		public float Flex, MinWidth;

		public float SafeMinWidth => float.IsNaN(MinWidth) ? 0 : MinWidth;

	}

	public class HeaderVisualElement : ContentManagerComponent
	{
		private SplitterVisualElement _splitter;

		public new class UxmlFactory : UxmlFactory<HeaderVisualElement, UxmlTraits> { }
		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlStringAttributeDescription customText = new UxmlStringAttributeDescription { name = "custom-text", defaultValue = "nada" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}
			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as HeaderVisualElement;
			}
		}

		//      private Label _nameLabel;

		//      public string Text { set; get; }

		public string[] Headers { get; set; }
		public event Action<List<HeaderSizeChange>> OnValuesChanged;

		public HeaderVisualElement() : base(nameof(HeaderVisualElement))
		{

		}

		public override void Refresh()
		{
			base.Refresh();

			if (Headers == null || Headers.Length == 0) return;

			_splitter = new SplitterVisualElement();
			Root.Add(_splitter);

			_splitter.OnFlexChanged += Header_OnFlexChanged;
			foreach (var header in Headers)
			{
				var lbl = new Label(header);
				var vs = new VisualElement();
				vs.AddToClassList("splitWrapper");
				vs.Add(lbl);
				_splitter.Add(vs);
			}

			//
			//         _nameLabel = Root.Q<Label>("nameLabel");
			//         _nameLabel.text = Text;
		}

		private void Header_OnFlexChanged(List<float> obj)
		{
			var children = _splitter.Children().ToArray();
			var output = new List<HeaderSizeChange>();
			for (var i = 0; i < obj.Count; i++)
			{
				var width = children[i].localBound.width;

				output.Add(new HeaderSizeChange
				{
					MinWidth = width,
					Flex = obj[i]
				});
			}
			OnValuesChanged?.Invoke(output);
		}

		public List<HeaderSizeChange> ComputeSizes(List<float> startFlexSizes)
		{
			var children = _splitter.Children().ToArray();
			var output = new List<HeaderSizeChange>();
			for (var i = 0; i < startFlexSizes.Count; i++)
			{
				children[i].style.flexGrow = startFlexSizes[i];

				var width = children[i].localBound.width;

				output.Add(new HeaderSizeChange
				{
					MinWidth = width,
					Flex = startFlexSizes[i]
				});
			}

			this.MarkDirtyRepaint();
			return output;
		}

		public void EmitFlexValues()
		{
			_splitter.EmitFlexValues();
		}
	}
}
