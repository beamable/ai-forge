using Beamable.Common;
using System.Collections.Generic;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants;

namespace Beamable.Editor.UI.Components
{
	public class LoadingIndicatorVisualElement : BeamableVisualElement
	{
		private Label _loadingLabel;

		public string LoadingText { get; private set; }

		private PromiseBase _promise;

		public LoadingIndicatorVisualElement() : base($"{Directories.COMMON_COMPONENTS_PATH}/{nameof(LoadingIndicatorVisualElement)}/{nameof(LoadingIndicatorVisualElement)}")
		{
		}

		public new class UxmlFactory : UxmlFactory<LoadingIndicatorVisualElement, UxmlTraits> { }
		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlStringAttributeDescription loadingText = new UxmlStringAttributeDescription { name = "text", defaultValue = "Loading" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}
			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as LoadingIndicatorVisualElement;

				self.LoadingText = loadingText.GetValueFromBag(bag, cc);
				self.LoadingText = string.IsNullOrEmpty(self.LoadingText)
				   ? loadingText.defaultValue
				   : self.LoadingText;

				self.Refresh();
			}
		}

		public override void Refresh()
		{
			base.Refresh();
			_loadingLabel = Root.Q<Label>();
			_loadingLabel.text = LoadingText;
		}

		public void SetText(string text)
		{
			LoadingText = text;
			_loadingLabel.text = text;
		}

		public LoadingIndicatorVisualElement SetPromise<T>(Promise<T> promise, params VisualElement[] coverElements)
		{
			_promise = promise;
			RemoveFromClassList("invisible");

			foreach (var coverElement in coverElements)
			{
				coverElement?.AddToClassList("cover");
			}
			promise.Then(_ =>
			{
				AddToClassList("invisible");
				foreach (var coverElement in coverElements)
				{
					coverElement?.RemoveFromClassList("cover");
				}
			});
			return this;
		}

	}
}
