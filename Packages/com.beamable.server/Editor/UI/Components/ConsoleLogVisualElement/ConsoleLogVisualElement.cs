using Beamable.Editor.UI.Model;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Microservice.UI.Components
{
	public class ConsoleLogVisualElement : MicroserviceComponent
	{
		private VisualElement _mainVisualElement;
		private VisualElement _statusIcon;
		private Label _time;
		private Label _description;
		private VisualElement _postfixIcon;
		private LogMessage _model;

		private int _descriptionChunkSize = 1000;

		public new class UxmlFactory : UxmlFactory<ConsoleLogVisualElement, UxmlTraits>
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
				var self = ve as ConsoleLogVisualElement;
			}
		}

		public override void Refresh()

		{
			base.Refresh();
			_mainVisualElement = Root.Q<VisualElement>("mainVisualElement");
			_statusIcon = Root.Q<VisualElement>("statusIcon");
			_time = Root.Q<Label>("time");
			_description = Root.Q<Label>("description");
			_postfixIcon = Root.Q<VisualElement>("postfixIcon");

		}

		public void SetNewModel(LogMessage model)
		{
			_model = model;

			if (string.IsNullOrWhiteSpace(model.Message))
				return;

			var text = model.Message.Split('\n');
			_description.text = text.Length > 0
				? text[0].SplitStringIntoParts(_descriptionChunkSize)[0]
				: model.Message.SplitStringIntoParts(_descriptionChunkSize)[0];

			if (!model.MessageColor.Equals(Color.clear))
				_description.style.color = model.MessageColor;

			_description.SetFontStyle(model.IsBoldMessage ? FontStyle.Bold : FontStyle.Normal);
			_time.text = model.Timestamp;
			SetIcon();
			SetPostfixMessageIcon(model.PostfixMessageIcon);
		}

		public void SetIcon()
		{
			const string DEBUG = "debug";
			const string INFO = "info";
			const string WARNING = "warning";
			const string ERROR = "error";
			_statusIcon.ClearClassList();
			switch (_model.Level)
			{
				case LogLevel.DEBUG:
					_statusIcon.AddToClassList(DEBUG);
					break;
				case LogLevel.INFO:
					_statusIcon.AddToClassList(INFO);
					break;
				case LogLevel.WARNING:
					_statusIcon.AddToClassList(WARNING);
					break;
				case LogLevel.FATAL:
				case LogLevel.ERROR:
				default:
					_statusIcon.AddToClassList(ERROR);
					break;
			}
		}

		public void SetPostfixMessageIcon(string postfixIcon)
		{
			_postfixIcon.ClearClassList();

			if (!string.IsNullOrEmpty(postfixIcon))
				_postfixIcon.AddToClassList(postfixIcon);
		}

		public ConsoleLogVisualElement() : base(nameof(ConsoleLogVisualElement))
		{

		}
	}
}
