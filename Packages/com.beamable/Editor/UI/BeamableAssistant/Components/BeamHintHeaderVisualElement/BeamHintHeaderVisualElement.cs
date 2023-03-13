using Beamable.Common.Assistant;
using Beamable.Editor.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine.Assertions;

#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Assistant
{
	/// <summary>
	/// This visual element handles the rendering of <see cref="BeamHint"/>s in the <see cref="BeamableAssistantWindow"/>.
	/// </summary>
	public class BeamHintHeaderVisualElement : BeamableAssistantComponent
	{
		public new class UxmlFactory : UxmlFactory<BeamHintHeaderVisualElement, UxmlTraits> { }

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlStringAttributeDescription customText = new UxmlStringAttributeDescription { name = "custom-text", defaultValue = "nada" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get
				{
					yield break;
				}
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as BeamHintHeaderVisualElement;
			}
		}

		/// <summary>
		/// The index into <see cref="BeamHintsDataModel.DisplayingHints"/> for the hint this element is currently rendering.
		/// </summary>
		private int _indexIntoDisplayingHints;

		/// <summary>
		/// Reference to a <see cref="BeamHintReflectionCache.Registry"/> that is used to access <see cref="BeamHintReflectionCache.ConverterData{T}"/> and correctly
		/// render the <see cref="BeamHintDetailsConfig"/>. 
		/// </summary>
		private readonly BeamHintReflectionCache.Registry _hintDetailsReflectionCache;

		/// <summary>
		/// Reference to the current existing <see cref="BeamHintsDataModel"/> that is feeding the <see cref="BeamableAssistantWindow"/>. 
		/// </summary>
		private readonly BeamHintsDataModel _hintDataModel;

		/// <summary>
		/// Cached copy of the <see cref="BeamHintHeader"/> for the hint we are displaying.
		/// </summary>
		private BeamHintHeader _displayingHintHeader;

		#region Visual Element References

		private Label _hintDisplayName;
		private Toggle _moreDetailsButton;
		private VisualElement _detailsContainer;
		private VisualElement _detailsBox;
		private Image _hintTypeIcon;
		private VisualElement _headerContainer;

		#endregion

		public BeamHintHeaderVisualElement() : base(nameof(BeamHintHeaderVisualElement)) { }

		public BeamHintHeaderVisualElement(BeamHintsDataModel dataModel,
										   BeamHintReflectionCache.Registry library,
										   in BeamHintHeader hint,
										   int headerIdx) : base(nameof(BeamHintHeaderVisualElement))
		{
			_hintDataModel = dataModel;
			_hintDetailsReflectionCache = library;
			UpdateFromBeamHintHeader(in hint, headerIdx);
		}

		public void UpdateFromBeamHintHeader(in BeamHintHeader hint, int headerIdx)
		{
			_displayingHintHeader = hint;
			_indexIntoDisplayingHints = headerIdx;
		}



		public sealed override void Refresh()
		{
			base.Refresh();

			var hintPrimaryDomain = Root.Q<Label>("hintPrimaryDomainLabel");
			_hintTypeIcon = Root.Q<Image>("hintTypeIcon");
			_hintDisplayName = Root.Q<Label>("hintDisplayName");
			_moreDetailsButton = Root.Q<Toggle>("moreDetailsButton");

			// Setup header container
			_headerContainer = Root.Q<VisualElement>("hintHeaderContainer");
			if (_indexIntoDisplayingHints % 2 == 1) _headerContainer.AddToClassList("oddRow");

			// Update the hint's label
			var hintTitle = _hintDetailsReflectionCache.TryGetHintTitleText(_displayingHintHeader, out var titleText) ? titleText : _displayingHintHeader.Id;
			_hintDisplayName.text = hintTitle;
#if UNITY_2020_1_OR_NEWER
			_hintDisplayName.style.textOverflow = TextOverflow.Ellipsis;
#endif

			// Update Hint Type Icon and Primary Domain
			var hintTypeClass = _displayingHintHeader.Type.ToString().ToLower();
			_hintTypeIcon.AddToClassList(hintTypeClass);

			_ = BeamHintDomains.TryGetDomainAtDepth(_displayingHintHeader.Domain, 0, out var primaryDomain);
			_ = _hintDetailsReflectionCache.TryGetDomainTitleText(primaryDomain, out var hintPrimaryDomainText);
			hintPrimaryDomain.text = hintPrimaryDomainText;
			hintPrimaryDomain.AddTextWrapStyle();

			// Find the ConverterData that is tied to the hint we are displaying from the HintDetails Reflection Cache system.
			var foundConverter = _hintDetailsReflectionCache.TryGetConverterDataForHint(_displayingHintHeader, out var converter);
			var hintDetailsConfig = foundConverter ? converter.HintConfigDetailsConfig : null;
			BeamHintTextMap textMap;
			if (foundConverter && converter.HintTextMap != null && converter.HintTextMap.TryGetHintTitle(_displayingHintHeader, out _) && converter.HintTextMap.TryGetHintIntroText(_displayingHintHeader, out _))
				textMap = converter.HintTextMap;
			else
				textMap = _hintDetailsReflectionCache.GetTextMapForId(_displayingHintHeader);

			// If there are no mapped converters, we don't display a more button since there would be no details to show.
			var detailsUxmlPath = hintDetailsConfig == null ? "" : hintDetailsConfig.UxmlFile;
			var detailsUssPaths = hintDetailsConfig == null ? new List<string>() : hintDetailsConfig.StylesheetsToAdd;

			// Setup details container and more button to not be visible
			_detailsContainer = Root.Q<VisualElement>("hintDetailsContainer");

			// If there are no configured UXML Path or a Converter tied to the matching HintDetailsVisualConfig, simply disable the button.
			if (hintDetailsConfig == null || string.IsNullOrEmpty(detailsUxmlPath) || textMap == null)
			{
				_moreDetailsButton.visible = false;
				_detailsContainer.AddToClassList("--positionHidden");
				_hintDataModel.DetailsOpenedHints.Remove(_displayingHintHeader);
			}
			else
			{
				if (!_hintDataModel.DetailsOpenedHints.Contains(_displayingHintHeader))
					_detailsContainer.AddToClassList("--positionHidden");

				_detailsBox = Root.Q<VisualElement>("hintDetailsBox");

				// Configure more button to display hint details container when pressed. 
				_moreDetailsButton.value = _hintDataModel.DetailsOpenedHints.Contains(_displayingHintHeader);
				_moreDetailsButton.text = _moreDetailsButton.value ? "Less" : "More";
				_moreDetailsButton.RegisterValueChangedCallback((changeEvt) =>
				{
					if (_hintDataModel.DetailsOpenedHints.Contains(_displayingHintHeader))
					{
						_detailsContainer.AddToClassList("--positionHidden");
						_hintDataModel.DetailsOpenedHints.Remove(_displayingHintHeader);
					}
					else
					{
						_detailsContainer.RemoveFromClassList("--positionHidden");
						_hintDataModel.DetailsOpenedHints.Add(_displayingHintHeader);
						_hintDataModel.DetailsOpenedHints = _hintDataModel.DetailsOpenedHints.Where(h => h.Type != BeamHintType.Invalid).Distinct().ToList();
					}

					_moreDetailsButton.text = changeEvt.newValue ? "Less" : "More";
				});

				// Ensure no null or empty paths exist in the configured USS files.
				var nonNullUssPaths = detailsUssPaths.Where(uss => !string.IsNullOrEmpty(uss)).ToList();

				// Ensure paths exist.
				Assert.IsTrue(File.Exists(detailsUxmlPath), $"Cannot find {detailsUxmlPath}");
				Assert.IsTrue(nonNullUssPaths.TrueForAll(File.Exists), $"Cannot find one of {string.Join(",", nonNullUssPaths)}");

				// Load UXML for details and add it to the details container.
				var detailsTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(detailsUxmlPath);
				_detailsBox.Add(detailsTreeAsset.CloneTree());
				foreach (var nonNullUssPath in nonNullUssPaths) _detailsBox.AddStyleSheet(nonNullUssPath);

				// Update Name and Notification Preferences
				{
					_detailsBox.Q<Label>("hintDetailsBoxHintDisplayName").text = hintTitle;
					var notificationToggle = _detailsBox.Q<Toggle>("hintDetailsBoxNotificationToggle");
					switch (_hintDataModel.GetHintNotificationValue(_displayingHintHeader))
					{
						case BeamHintNotificationPreference.NotifyOncePerSession:
						case BeamHintNotificationPreference.NotifyOnContextObjectChanged:
							notificationToggle.SetValueWithoutNotify(true);
							break;
						case BeamHintNotificationPreference.NotifyNever:
							notificationToggle.SetValueWithoutNotify(false);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					notificationToggle.RegisterValueChangedCallback(evt => _hintDataModel.SetHintNotificationValue(_displayingHintHeader, evt.newValue));
				}

				// Update Play-Mode Preferences
				{
					var playModeNever = _detailsBox.Q<Toggle>("playModeWarningDisableToggle");
					playModeNever.Q<Label>().AddTextWrapStyle();
					var playModeSession = _detailsBox.Q<Toggle>("playModeWarningSessionToggle");
					playModeSession.Q<Label>().AddTextWrapStyle();
					var playModeAlways = _detailsBox.Q<Toggle>("playModeWarningAlwaysToggle");
					playModeAlways.Q<Label>().AddTextWrapStyle();
					var playModeState = _hintDataModel.GetHintPlayModeWarningState(_displayingHintHeader);
					if (playModeState == BeamHintPlayModeWarningPreference.Enabled)
					{
						playModeAlways.SetValueWithoutNotify(true);

						playModeSession.SetValueWithoutNotify(false);
						playModeNever.SetValueWithoutNotify(false);
					}
					else if (playModeState == BeamHintPlayModeWarningPreference.EnabledDuringSession)
					{
						playModeSession.SetValueWithoutNotify(true);

						playModeNever.SetValueWithoutNotify(false);
						playModeAlways.SetValueWithoutNotify(false);
					}
					else if (playModeState == BeamHintPlayModeWarningPreference.Disabled)
					{
						playModeNever.SetValueWithoutNotify(true);

						playModeSession.SetValueWithoutNotify(false);
						playModeAlways.SetValueWithoutNotify(false);
					}

					playModeNever.RegisterValueChangedCallback(_ =>
					{
						if (!playModeAlways.value && !playModeSession.value) playModeNever.SetValueWithoutNotify(true);
						_hintDataModel.SetHintPreferencesValue(_displayingHintHeader, BeamHintPlayModeWarningPreference.Disabled);

						playModeAlways.SetValueWithoutNotify(false);
						playModeSession.SetValueWithoutNotify(false);
					});
					playModeSession.RegisterValueChangedCallback(evt =>
					{
						if (!playModeAlways.value && !playModeNever.value) playModeSession.SetValueWithoutNotify(true);
						_hintDataModel.SetHintPreferencesValue(_displayingHintHeader, BeamHintPlayModeWarningPreference.EnabledDuringSession);

						playModeAlways.SetValueWithoutNotify(false);
						playModeNever.SetValueWithoutNotify(false);
					});
					playModeAlways.RegisterValueChangedCallback(evt =>
					{
						if (!playModeSession.value && !playModeNever.value) playModeAlways.SetValueWithoutNotify(true);
						_hintDataModel.SetHintPreferencesValue(_displayingHintHeader, BeamHintPlayModeWarningPreference.Enabled);

						playModeSession.SetValueWithoutNotify(false);
						playModeNever.SetValueWithoutNotify(false);
					});
				}

				// Create an new injection bag
				var injectionBag = new BeamHintVisualsInjectionBag();

				// Call the converter to fill up this injection bag.
				var beamHint = _hintDataModel.GetHint(_displayingHintHeader);
				converter.ConverterCall.Invoke(in beamHint, in textMap, injectionBag);

				// Resolve all supported injections.
				ResolveInjections(injectionBag.TextInjections, _detailsBox);
				ResolveInjections(injectionBag.ParameterlessActionInjections, _detailsBox);
				ResolveInjections(injectionBag.DynamicVisualElementInjections, _detailsBox);
			}
		}

		/// <summary>
		/// Query the given <paramref name="container"/> for matching elements to each of the given <paramref name="injections"/>.
		/// Matches by VisualElement type, name and class, then calls <see cref="Inject{T}"/> for all matching elements. 
		/// </summary>
		/// <param name="injections">A collection of <see cref="BeamHintVisualsInjectionBag.Injection{T}"/>s that were created in a <see cref="BeamHintDetailConverterAttribute"/> annotated function.</param>
		/// <param name="container">The <see cref="VisualElement"/> for us to run the injections in.</param>
		/// <typeparam name="T">Any type as defined in <see cref="BeamHintVisualsInjectionBag"/> fields.</typeparam>
		public void ResolveInjections<T>(IEnumerable<BeamHintVisualsInjectionBag.Injection<T>> injections, VisualElement container)
		{
			foreach (var injection in injections)
			{
				// Finds all matching elements
				var query = injection.Query;
				var queryExpectedType = query.ExpectedType;
				var queriedElements = container
									  .Query(query.Name, query.Classes)
									  .Where(element => element.GetType() == queryExpectedType)
									  .Build()
									  .ToList();

				Debug.Assert(queriedElements.Count != 0,
							 $"Query [{query}] found no matches when searching in the {nameof(VisualElement)} [{container.name}]");

				Debug.Assert(queriedElements.TrueForAll(element => element.GetType() == queryExpectedType),
							 $"Query [{query}] does not match its expected type when searching in the {nameof(VisualElement)} [{container.name}]");

				// For each found element, inject based on the type of element and the type of the injection
				foreach (var queriedElement in queriedElements)
					Inject(queriedElement, injection);
			}
		}

		/// <summary>
		/// Based on all supported <see cref="VisualElement"/> types, we call the appropriate injection function that'll configure the matched element appropriately.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Inject<T>(VisualElement matchedElement, BeamHintVisualsInjectionBag.Injection<T> toInject)
		{
			switch (matchedElement)
			{
				case Button button:
				{
					ResolveButtonInjection(toInject, button);
					break;
				}
				case Label label:
				{
					ResolveLabelInjection(toInject, label);
					break;
				}
				case VisualElement container:
				{
					ResolveContainerInjection(toInject, container);
					break;
				}
				default:
					throw new ArgumentException($"Unsupported Injection! The system doesn't know how inject into a VisualElement of type {matchedElement.GetType().Name}.");
			}
		}

		/// <summary>
		/// Resolve supported injections for <see cref="Label"/> <see cref="VisualElement"/>s.
		/// </summary>
		private static void ResolveLabelInjection<T>(BeamHintVisualsInjectionBag.Injection<T> toInject, Label label)
		{
			switch (toInject.ObjectToInject)
			{
				case Action clicked:
				{
					label.RegisterCallback(new EventCallback<MouseUpEvent>(evt => clicked?.Invoke()));
					break;
				}
				case string text:
				{
					label.text = text;
					label.AddTextWrapStyle();
					break;
				}
				default:
					throw new ArgumentException($"Unsupported Injection! The system doesn't know how inject object of type {typeof(T).Name} into a {nameof(Label)}.");
			}
		}

		/// <summary>
		/// Resolve supported injections for <see cref="Button"/> <see cref="VisualElement"/>s.
		/// </summary>
		private static void ResolveButtonInjection<T>(BeamHintVisualsInjectionBag.Injection<T> toInject, Button button)
		{
			switch (toInject.ObjectToInject)
			{
				case Action clicked:
				{
					button.clickable.clicked += clicked;
					break;
				}
				case string label:
				{
					button.text = label;
					break;
				}
				default:
					throw new ArgumentException($"Unsupported Injection! The system doesn't know how inject object of type {typeof(T).Name} into a {nameof(Button)}.");
			}
		}

		/// <summary>
		/// Resolve supported injections for <see cref="Button"/> <see cref="VisualElement"/>s.
		/// </summary>
		private static void ResolveContainerInjection<T>(BeamHintVisualsInjectionBag.Injection<T> toInject, VisualElement container)
		{
			switch (toInject.ObjectToInject)
			{
				case string label:
				{
					container.Add(new Label(label));
					break;
				}
				case VisualElement dynamicElement:
				{
					container.Add(dynamicElement);
					break;
				}
				default:
					throw new ArgumentException($"Unsupported Injection! The system doesn't know how inject object of type {typeof(T).Name} into a {nameof(Button)}.");
			}
		}
	}

	/// <summary>
	/// A struct defining the data we use to match against <see cref="VisualElement"/>s and select which of them we want to inject with the data
	/// from <see cref="BeamHintVisualsInjectionBag"/>. 
	/// </summary>
	public readonly struct VisualElementsQuery
	{
		/// <summary>
		/// A sub-class of <see cref="VisualElement"/> (or itself).
		/// </summary>
		public readonly Type ExpectedType;

		/// <summary>
		/// The name property of any <see cref="VisualElement"/> you wish to find.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// One or more classes that'll be matched against <see cref="VisualElement"/>s' classes. 
		/// </summary>
		public readonly string[] Classes;

		public VisualElementsQuery(Type expectedType, string name, string[] classes)
		{
			ExpectedType = expectedType;
			Name = name;
			Classes = classes;
		}

		public override string ToString()
		{
			return $"{nameof(ExpectedType)}: {ExpectedType}, {nameof(Name)}: {Name}, {nameof(Classes)}: {Classes}";
		}
	}

	/// <summary>
	/// A clear definition of all supported injection types. An instance of this is passed into every <see cref="BeamHintDetailConverterAttribute"/> function.
	/// <para/>
	/// It exposes clear helper functions such as <see cref="SetLabel"/> and <see cref="SetButtonClicked"/> to ensure the internal <see cref="Injection{T}"/> data is being configured
	/// correctly. 
	/// </summary>
	public class BeamHintVisualsInjectionBag
	{
		public readonly IEnumerable<Injection<string>> TextInjections;
		public readonly IEnumerable<Injection<Action>> ParameterlessActionInjections;
		public readonly IEnumerable<Injection<VisualElement>> DynamicVisualElementInjections;

		private readonly List<Injection<string>> _textInjections;
		private readonly List<Injection<Action>> _parameterlessActionInjections;
		private readonly List<Injection<VisualElement>> _dynamicVisualElementInjections;


		public BeamHintVisualsInjectionBag()
		{
			_textInjections = new List<Injection<string>>();
			_parameterlessActionInjections = new List<Injection<Action>>();
			_dynamicVisualElementInjections = new List<Injection<VisualElement>>();
			TextInjections = new TextIterator(this);
			ParameterlessActionInjections = new ParameterlessActionIterator(this);
			DynamicVisualElementInjections = new DynamicVisualElementIterator(this);
		}

		public readonly struct Injection<T>
		{
			public readonly VisualElementsQuery Query;
			public readonly T ObjectToInject;

			public Injection(VisualElementsQuery query, T objectToInject)
			{
				Query = query;
				ObjectToInject = objectToInject;
			}
		}

		public class TextIterator : IEnumerable<Injection<string>>
		{
			public readonly BeamHintVisualsInjectionBag bag;

			public TextIterator(BeamHintVisualsInjectionBag beamHintVisualsInjectionBag)
			{
				bag = beamHintVisualsInjectionBag;
			}

			public IEnumerator<Injection<string>> GetEnumerator()
			{
				return bag._textInjections.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public class ParameterlessActionIterator : IEnumerable<Injection<Action>>
		{
			public readonly BeamHintVisualsInjectionBag bag;

			public ParameterlessActionIterator(BeamHintVisualsInjectionBag beamHintVisualsInjectionBag)
			{
				bag = beamHintVisualsInjectionBag;
			}

			public IEnumerator<Injection<Action>> GetEnumerator()
			{
				return bag._parameterlessActionInjections.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public class DynamicVisualElementIterator : IEnumerable<Injection<VisualElement>>
		{
			public readonly BeamHintVisualsInjectionBag bag;

			public DynamicVisualElementIterator(BeamHintVisualsInjectionBag beamHintVisualsInjectionBag)
			{
				bag = beamHintVisualsInjectionBag;
			}

			public IEnumerator<Injection<VisualElement>> GetEnumerator()
			{
				return bag._dynamicVisualElementInjections.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public void SetButtonLabel(string buttonLabel, string name, params string[] classes)
		{
			_textInjections.Add(new Injection<string>(new VisualElementsQuery(typeof(Button), name, classes), buttonLabel));
		}

		public void SetButtonClicked(Action buttonAction, string name, params string[] classes)
		{
			_parameterlessActionInjections.Add(new Injection<Action>(new VisualElementsQuery(typeof(Button), name, classes), buttonAction));
		}

		public void SetLabel(string buttonLabel, string name, params string[] classes)
		{
			_textInjections.Add(new Injection<string>(new VisualElementsQuery(typeof(Label), name, classes), buttonLabel));
		}

		public void SetLabelClicked(Action buttonAction, string name, params string[] classes)
		{
			_parameterlessActionInjections.Add(new Injection<Action>(new VisualElementsQuery(typeof(Label), name, classes), buttonAction));
		}

		public void AddAsChild(VisualElement element, string containerName, params string[] containerClasses)
		{
			_dynamicVisualElementInjections.Add(new Injection<VisualElement>(new VisualElementsQuery(typeof(VisualElement), containerName, containerClasses), element));
		}
	}
}
