using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;
using Random = UnityEngine.Random;
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
	public class LoadingBarElement : BeamableVisualElement, ILoadingBar
	{

		public new class UxmlFactory : UxmlFactory<LoadingBarElement, LoadingBarElement.UxmlTraits>
		{
		}

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlStringAttributeDescription customText = new UxmlStringAttributeDescription
			{ name = "custom-text", defaultValue = "nada" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as LoadingBarElement;

			}
		}

		private static Texture _animationTexture;

		public LoadingBarElement() : base($"{Directories.COMMON_COMPONENTS_PATH}/{nameof(LoadingBarElement)}/{nameof(LoadingBarElement)}") { }

		private VisualElement _fillElement;
		private Label _label;
		private IMGUIContainer _animation;
		private Button _button;
		private LoadingBarUpdater _updater;
		private bool _smallBar;
		private float _animationOffset;
		public event Action OnUpdated;

		public LoadingBarUpdater Updater => _updater;

		public bool SmallBar
		{
			get => _smallBar;
			set
			{
				if (_smallBar == value) return;
				_smallBar = value;
				UpdateClasses();
			}
		}

		public bool RunWithoutUpdater { get; set; }

		private float _progress;
		public float Progress
		{
			get => _progress;
			set
			{
				_progress = Mathf.Clamp01(value);
				OnUpdated?.Invoke();
				SyncWidth();
			}
		}

		public string Message
		{
			get => _label.text;
			set => _label.text = value;
		}

		private bool _failed;
		public bool Failed
		{
			get => _failed;
			set
			{
				if (_failed == value) return;
				_failed = value;
				UpdateClasses();
			}
		}

		private bool _hidden;

		public bool Hidden
		{
			get => _hidden;
			set
			{
				_hidden = value;
				UpdateClasses();
			}
		}

		public override void Refresh()
		{
			base.Refresh();

			_fillElement = Root.Q("fill");
			_label = Root.Q<Label>("label");
			_button = Root.Q<Button>("button");
			if (_button == null)
			{
				_button = new Button { name = "button", text = "Confirm" };
				_label.parent.Add(_button);
				_button.BringToFront();
			}
			_animationOffset = Random.Range(0.0f, 1.0f);

			RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
			UpdateClasses();

			_animation = new IMGUIContainer(AnimationOnGUIHandler) { name = "animation" };
			_fillElement.parent.Add(_animation);
			_animation.PlaceInFront(_fillElement);
			EditorApplication.update += Update;


			_button.clickable.clicked -= OnButtonClick;
			_button.clickable.clicked += OnButtonClick;
		}

		protected override void OnDestroy()
		{
			EditorApplication.update -= Update;
		}

		private void Update()
		{
			var animationVisible = !Hidden && (RunWithoutUpdater || (_updater != null && !_updater.Killed));

			if (animationVisible && !_animation.visible)
				_animationOffset = Random.value;

			_animation.visible = animationVisible;
			if (_animation.visible)
			{
				_animation.MarkDirtyRepaint();
			}

			_button.visible = !Hidden && !SmallBar && !_animation.visible;
		}

		private void AnimationOnGUIHandler()
		{
			if (_animationTexture == null)
			{
				_animationTexture =
					EditorResources.Load<Texture>("Packages/com.beamable/Editor/UI/Common/Icons/loading_animation.png");
			}
			var rect = EditorGUILayout.GetControlRect(false, layout.height);
			var time = (float)((EditorApplication.timeSinceStartup * .7) % 1);
			GUI.DrawTextureWithTexCoords(rect, _animationTexture,
				new Rect(-time + _animationOffset, 0, 1.2f, 1));
		}

		private void OnGeometryChanged(GeometryChangedEvent _)
		{
			SyncWidth();
		}

		private void OnButtonClick()
		{
			Hidden = true;
		}

		public void UpdateProgress(float progress, string message = null, bool failed = false, bool hideOnFinish = false)
		{
			Progress = progress;
			Message = message;
			Failed = failed;
			Hidden = hideOnFinish || progress >= 1f;
			if (Hidden)
			{
				_updater?.Kill();
			}
		}

		public void SetUpdater(LoadingBarUpdater updater)
		{
			if (updater == _updater) return;
			_updater?.Kill();
			_updater = updater;
			Failed = false;
		}


		private void SyncWidth()
		{
			try
			{
#if UNITY_2018
                var layout = Root.layout;
                layout.position = Vector2.zero;
                layout.width = Root.layout.width * Progress;
                layout.height = Root.layout.height;
                _fillElement.layout = layout;
#elif UNITY_2019_1_OR_NEWER
                _fillElement.style.width = layout.width * Progress;
                        _fillElement.style.height =
                            layout.height;
#endif
			}
			catch (UnityException e)
			{
				if (e.Message.Contains("thread"))
				{
					EditorApplication.delayCall += SyncWidth;
				}
				else
				{
					throw e;
				}
			}
		}

		private void UpdateClasses()
		{
			if (_smallBar)
			{
				AddToClassList("smallBar");
			}
			else
			{
				RemoveFromClassList("smallBar");
			}
			if (Failed)
			{
				AddToClassList("failed");
			}
			else
			{
				RemoveFromClassList("failed");
			}
			if (Hidden)
			{
				AddToClassList("hidden");
			}
			else
			{
				RemoveFromClassList("hidden");
			}
		}
	}
}
