using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS0618

namespace Beamable.Common.Content.Validation
{
	public class ContentExceptionCollection
	{
		public List<ContentException> Exceptions;
		public IContentObject Content;

		public bool AnyExceptions => (Exceptions?.Count ?? 0) > 0;
		public ContentExceptionCollection(IContentObject content)
		{
			Content = content;
			Exceptions = new List<ContentException>();
		}
		public ContentExceptionCollection(IContentObject content, List<ContentException> exceptions)
		{
			Content = content;
			Exceptions = exceptions;
		}
	}

	public class ContentException : Exception
	{
		public IContentObject Content { get; }

		public ContentException(IContentObject content, string message) : base(message)
		{
			Content = content;
		}

		public virtual string FriendlyMessage => Message;
	}

	[Agnostic]
	public class ContentValidationException : ContentException
	{
		private readonly string _message;
		public ValidationFieldWrapper Info { get; }

		public ContentValidationException(IContentObject content, ValidationFieldWrapper info) : base(content, generateMessage(info, ""))
		{
			Info = info;
		}

		public ContentValidationException(IContentObject content, ValidationFieldWrapper info, string message) : base(content, generateMessage(info, $" {message}"))
		{
			_message = message;
			Info = info;
		}

		public override string FriendlyMessage => _message;

		private static string generateMessage(ValidationFieldWrapper info, string message)
		{
			return $"Error with '{info.FieldType} {info.Field.Name}' on {info.Field.DeclaringType}.{message}";
		}
	}

	public class ContentNameValidationException : ContentException
	{
		public static Dictionary<char, string> INVALID_CHARACTERS = new Dictionary<char, string>
	  {
		 {' ', "spaces"},
		 {'/', "forward slash"},
		 {'\\', "back slashes"},
		 {'|', "pipes"},
		 {':', "colons"},
		 {'*', "asterisks"},
		 {'?', "question marks"},
		 {'"', "double quotes"},
		 {'<', "less than symbols"},
		 {'>', "greater than symbols"},
	  };

		public static char[] INVALID_CHARS = INVALID_CHARACTERS.Keys.ToArray();

		public char InvalidChar { get; }
		public int InvalidCharPosition { get; }
		public string Name { get; }
		public string InvalidCharName { get; }
		public ContentNameValidationException(IContentObject content, char invalidChar, string invalidCharName, int position, string name)
		   : base(content, $"Content name=[{name}] cannot contain {invalidCharName} at position=[{position}]")
		{
			InvalidChar = invalidChar;
			InvalidCharName = invalidCharName;
			InvalidCharPosition = position;
			Name = name;
		}

		public static bool HasNameValidationErrors(IContentObject content, string contentName, out List<ContentNameValidationException> errors)
		{
			errors = null;

			for (var i = 0; i < contentName.Length; i++)
			{
				var badCharIndex = Array.IndexOf(INVALID_CHARS, contentName[i]);
				if (badCharIndex >= 0)
				{
					var invalidChar = INVALID_CHARS[badCharIndex];

					if (errors == null)
						errors = new List<ContentNameValidationException>();

					errors.Add(new ContentNameValidationException(content,
					   invalidChar,
					   INVALID_CHARACTERS[invalidChar],
					   i,
					   contentName));
				}
			}

			return errors?.Count > 0;
		}
	}
}
