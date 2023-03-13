using Beamable.Editor.UI.Components;
using System.IO;
using UnityEngine.Assertions;
using Beamable.Common.Dependencies;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants;

namespace Beamable.Editor.UI.Common
{
	public class BeamableBasicVisualElement : VisualElement
	{
		protected VisualElement Root { get; set; }
		protected string UssPath { get; }
		private readonly bool _createRoot;

		private IDependencyProvider _provider;

		/// <summary>
		/// A <see cref="IDependencyProvider"/> associated with the context that is executing the UI.
		/// By default, this will be the default context's dependency scope. However, you can use the <see cref="Refresh(IDependencyProvider)"/>
		/// method to override the <see cref="Provider"/> property for a branch of the UI Hierarchy.
		/// </summary>
		public IDependencyProvider Provider
		{
			get
			{
				if (_provider != null) return _provider; // we have a configured provider, use that.
				if (!(parent is BeamableBasicVisualElement beamParent))
				{
					return BeamEditorContext.Default.ServiceScope; // we don't have a parent, and since we didn't have a provider, use the default.
				}
				return beamParent.Provider; // our parent is another thing that could have a provider, so defer to that.
			}
		}

		public BeamEditorContext Context => Provider.GetService<BeamEditorContext>();

		protected BeamableBasicVisualElement(string ussPath, bool createRoot = true)
		{
			Assert.IsTrue(File.Exists(ussPath), $"Cannot find {ussPath}");

			UssPath = ussPath;
			_createRoot = createRoot;
			RegisterCallback<DetachFromPanelEvent>(evt =>
			{
				OnDetach();
			});
		}

		public void Refresh(IDependencyProvider provider)
		{
			_provider = provider;
			Refresh();
		}

		public virtual void Refresh() { }

		protected virtual void OnDestroy() { }

		protected virtual void OnDetach() { }

		public virtual void Init()
		{
			Clear();

			this.AddStyleSheet(Files.COMMON_USS_FILE);
			this.AddStyleSheet(UssPath);

			if (_createRoot)
			{
				Root = new VisualElement();
				Root.name = "root";
				Add(Root);
			}
			else
			{
				Root = this;
			}

			this?.Query<VisualElement>(className: "--image-scale-to-fit").ForEach(elem =>
			{
				elem?.SetBackgroundScaleModeToFit();
			});
		}

		public void Destroy()
		{
			foreach (var child in Children())
			{
				if (child is BeamableVisualElement beamableChild)
				{
					beamableChild.Destroy();
				}

				if (child is BeamableBasicVisualElement beamableBasicChild)
				{
					beamableBasicChild.Destroy();
				}
			}

			OnDestroy();
		}
	}
}
