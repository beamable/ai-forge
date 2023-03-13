using Beamable.Common;
using Beamable.Editor.UI.Common;
using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
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
	public class PrimaryButtonVisualElement : BeamableVisualElement
	{
		private static char[] _digitChars = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
		private LoadingSpinnerVisualElement _spinner;

		private Dictionary<string, bool> _fieldValid = new Dictionary<string, bool>();
		private List<FormConstraint> _constraints = new List<FormConstraint>();

		private const string CLASS_NAME_REGEX = "^[A-Za-z_][A-Za-z0-9_]*$";
		private const string MANIFEST_NAME_REGEX = @"(^[0-9]*$)|(^[a-z][a-z0-9\-]*$)";
		private const string ALIAS_REGEX = "^[a-z][a-z0-9-]*$";

		public string Text { get; private set; }

		public Button Button { get; private set; }

		public PrimaryButtonVisualElement() : base($"{Directories.COMMON_COMPONENTS_PATH}/{nameof(PrimaryButtonVisualElement)}/{nameof(PrimaryButtonVisualElement)}")
		{
		}

		public new class UxmlFactory : UxmlFactory<PrimaryButtonVisualElement, UxmlTraits> { }
		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlStringAttributeDescription text = new UxmlStringAttributeDescription { name = "text", defaultValue = "Continue" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}
			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as PrimaryButtonVisualElement;

				self.Text = text.GetValueFromBag(bag, cc);
				self.Text = string.IsNullOrEmpty(self.Text)
				   ? text.defaultValue
				   : self.Text;

				self.Refresh();
			}
		}

		public void SetText(string text)
		{
			Text = text;
			Button.text = text;
		}

		public bool CheckGateKeepers()
		{
			foreach (var constraint in _constraints)
			{
				constraint.Check(true);
			}

			return Button.enabledSelf;
		}

		public void AddGateKeeper(params FormConstraint[] constraints)
		{
			foreach (var constraint in constraints)
			{
				_constraints.Add(constraint);
				constraint.OnValidate += _ => CheckConstraints(constraint);

				CheckConstraints(constraint);
			}
		}

		void CheckConstraints(FormConstraint src)
		{
			var valid = _constraints.All(v => v.IsValid);
			_fieldValid[name] = valid;

			for (var i = 0; i < _constraints.Count; i++)
			{
				if (_constraints[i] == src) continue;
				_constraints[i].Notify();
			}

			var missingFields = _constraints.Where(kvp => !kvp.IsValid).Select(kvp => kvp.Name).ToList();
			if (missingFields.Count == 0)
			{
				tooltip = "";
				Enable();
			}
			else
			{
				Disable();
				tooltip = $"Required: {string.Join(",", missingFields)}";
			}
		}

		public void Disable()
		{
			Button.SetEnabled(false);
			AddToClassList("disabled");
		}

		public void Enable()
		{
			RemoveFromClassList("disabled");
			Button.SetEnabled(true);
		}

		public void SetAsFailure(bool failure = true)
		{
			const string failureClass = "failure";
			EnableInClassList(failureClass, failure);
		}

		public void Load<T>(Promise<T> promise)
		{
			AddToClassList("loading");
			var startText = Button.text;
			Button.text = "";
			void Finish()
			{
				Button.text = startText;
				RemoveFromClassList("loading");
			}

			promise
			   .Then(_ => Finish())
			   .Error(_ => Finish());
		}

		public override void Refresh()
		{
			base.Refresh();
			Button = Root.Q<Button>();
			Button.text = Text;

			_spinner = Root.Q<LoadingSpinnerVisualElement>();

			Button.RegisterCallback<GeometryChangedEvent>(evt =>
			{
				SetSpinnerLocation();
			});
			EditorApplication.delayCall += SetSpinnerLocation;
		}

		private void SetSpinnerLocation()
		{
			var coef = .5f;
#if UNITY_2018
         coef = 1;
#endif
			_spinner.style.SetLeft(Button.worldBound.width * .5f - _spinner.Size * coef);
			_spinner.style.SetTop(Button.worldBound.height * .5f - _spinner.Size * coef);
		}

		public static string AliasErrorHandler(string alias)
		{
			if (string.IsNullOrEmpty(alias)) return "Alias is required";
			if (!IsValidAlias(alias)) return "Alias must be at least 2 characters long and must start with a lowercase letter, contain only lower case letters, numbers or dashes";
			return null;
		}

		public static string AliasOrCidErrorHandler(string aliasOrCid)
		{
			if (string.IsNullOrEmpty(aliasOrCid)) return "Alias or CID required";

			// if there are leading numbers, this is a CID. Otherwise, it is an alias.
			var isCid = _digitChars.Contains(aliasOrCid[0]);

			if (!isCid) return AliasErrorHandler(aliasOrCid);

			// there can only be digits...
			var replaced = Regex.Replace(aliasOrCid, @"^\d+", "");
			if (replaced.Length > 0)
			{
				return "CID can only contain numbers";
			}

			return null;
		}

		public static string GameNameErrorHandler(string gameName)
		{
			return IsGameNameValid(gameName, out var errorMessage)
			   ? null
			   : errorMessage;
		}
		public static string ExistErrorHandler(string field)
		{
			if (string.IsNullOrEmpty(field)) return "Required";
			return null;
		}

		public static string EmailErrorHandler(string email)
		{
			return PrimaryButtonVisualElement.IsValidEmail(email)
			   ? null
			   : "Email is not valid";
		}

		public static string PasswordErrorHandler(string password)
		{
			return PrimaryButtonVisualElement.IsPassword(password)
			   ? null
			   : "A valid password must be at least 4 characters long";
		}

		public static string LegalErrorHandler(bool read)
		{
			return read ? null : "Must agree to legal terms";
		}

		public static string IsBetweenCharLength(string text, int max, int min = -1)
		{
			if (text.Length > max) return $"Must be ${max} characters or less";
			if (text.Length < min) return $"Must be ${min} characters or more";
			return null;
		}

		public static string IsValidClassName(string name)
		{
			var codeProvider = new CSharpCodeProvider();
			string sFixedName = codeProvider.CreateValidIdentifier(name);
			var codeType = new CodeTypeDeclaration(sFixedName);

			if (!string.Equals(codeType.Name, name))
			{
				return "Cannot use reserved C# words";
			}

			if (!System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(name) ||
				!Regex.IsMatch(name, CLASS_NAME_REGEX))
			{
				return "Must be a valid C# class name";
			}
			return null;
		}

		public static bool IsPassword(string password)
		{
			return password.Length > 1; // TODO: Implement actual password check
		}
		public static Func<string, bool> MatchesTextField(TextField tf)
		{
			return (str => string.Equals(tf.value, str));
		}

		private static bool IsValidAlias(string alias)
		{
			if (alias == null) return false;
			bool isMatch = Regex.IsMatch(alias, ALIAS_REGEX);
			return alias.Length > 1 && isMatch;
		}

		public static bool IsSlug(string slug)
		{
			if (slug == null) return false;
			bool isMatch = Regex.IsMatch(slug, MANIFEST_NAME_REGEX);
			return slug.Length > 1 && isMatch;
		}

		public static bool IsGameNameValid(string gameName, out string errorMessage)
		{
			errorMessage = string.Empty;
			if (string.IsNullOrWhiteSpace(gameName))
			{
				errorMessage = "Game name is required";
			}
			else if (gameName.Length < 3)
			{
				errorMessage = "A valid game name must be at least 3 characters long";
			}
			else if (gameName.Length > 40)
			{
				errorMessage = "A valid game name must be no longer than 40 characters long";
			}
			else if (!Regex.IsMatch(gameName, "^[a-zA-Z0-9-_ ]+$"))
			{
				errorMessage = "Game name can contain letters, numbers, dashes and spaces";
			}
			return errorMessage == string.Empty;
		}
		public static bool IsValidEmail(string email)
		{
			try
			{
				email = email.Trim();
				var addr = new System.Net.Mail.MailAddress(email);
				return addr.Address == email;
			}
			catch
			{
				return false;
			}
		}
	}
}
