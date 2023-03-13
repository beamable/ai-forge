using System;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Common.Content
{

	/// <summary>
	/// A <see cref="ContentQuery"/> allows you to construct a complex statement that selects specific content by type, tag, or id.
	/// </summary>
	public class ContentQuery : DefaultQuery
	{
		/// <summary>
		/// A <see cref="ContentQuery"/> that selects every piece of content.
		/// </summary>
		public static readonly ContentQuery Unit = new ContentQuery();

		/// <summary>
		/// Type constraints instruct the <see cref="ContentQuery"/> to select content that matches the given types.
		/// In string form, this can be represented as "t:items", or "t:announcements".
		/// Multiple type constraints are treated as an OR operation. As a string, "t:items announcements" will select all
		/// items and announcements.
		/// </summary>
		public HashSet<Type> TypeConstraints;

		/// <summary>
		/// Tag constraints instruct the <see cref="ContentQuery"/> to select content that contain the given tags.
		/// In string form, this can be represented as "tag:a", or "tag:a b".
		/// Multiple tag constraints are treated as an AND operation. As a string, "tag:a b" will only match content that have
		/// both the "a" tag, and the "b" tag.
		/// </summary>
		public HashSet<string> TagConstraints;

		public ContentQuery()
		{

		}

		/// <summary>
		/// The copy-constructor for a <see cref="ContentQuery"/>
		/// </summary>
		/// <param name="other">A <see cref="ContentQuery"/> to clone. The given instance will not be mutated.</param>
		public ContentQuery(ContentQuery other)
		{
			if (other == null) return;

			TypeConstraints = other.TypeConstraints != null
			   ? new HashSet<Type>(other.TypeConstraints.ToArray())
			   : null;
			TagConstraints = other.TagConstraints != null
			   ? new HashSet<string>(other.TagConstraints.ToArray())
			   : null;
			IdContainsConstraint = other.IdContainsConstraint;
		}

		protected static void ApplyTypeParse(string raw, ContentQuery query)
		{
			try
			{
				var typeNames = raw.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				var types = new HashSet<Type>();
				foreach (var typeName in typeNames)
				{
					try
					{
						var type = ContentTypeReflectionCache.Instance.NameToType(typeName);
						types.Add(type);
					}
					catch (Exception ex)
					{
						BeamableLogger.LogException(ex);
					}
				}
				query.TypeConstraints = new HashSet<Type>(types);

			}
			catch (Exception)
			{
				// don't do anything.
				//query.TypeConstraint = typeof(int); // something to block filtering from working.
			}
		}


		protected static void ApplyTagParse(string raw, ContentQuery query)
		{
			var tags = raw.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			query.TagConstraints = new HashSet<string>(tags);
		}

		protected static readonly Dictionary<string, DefaultQueryParser.ApplyParseRule<ContentQuery>> StandardRules = new Dictionary<string, DefaultQueryParser.ApplyParseRule<ContentQuery>>
	  {
		 {"t", ApplyTypeParse},
		 {"id", DefaultQueryParser.ApplyIdParse},
		 {"tag", ApplyTagParse},
	  };

		protected static readonly List<DefaultQueryParser.SerializeRule<ContentQuery>> StandardSerializeRules = new List<DefaultQueryParser.SerializeRule<ContentQuery>>
	  {
		 SerializeTagRule, SerializeTypeRule, SerializeIdRule
	  };

		/// <summary>
		/// Produce a <see cref="ContentQuery"/> from its string format. The string can contain many clauses for the allowed constraints.
		/// <para>
		/// To specify type constraints, add a "t:" clause, like "t:items". You can select multiple types by specifying multiple
		/// types as space separated in the string, like "t:items announcements".
		/// The resulting types will fill out the <see cref="TypeConstraints"/> field.
		/// </para>
		/// <para>
		/// To specify tag constraints, add a "tag:" clause, like "tag:a". You can select content that have many required tags
		/// by adding more tags as space separated in the string, like "t:a b".
		/// The resulting tags will fill out the <see cref="TagConstraints"/> field.
		/// </para>
		/// <para>
		/// To specify an id constraint, add a "id:" clause, or specify the id as a standalone string. Both "id:items.hat" and "items.hat"
		/// produce the same output. The id will fill the <see cref="DefaultQuery.IdContainsConstraint"/> field.
		/// Ids are substring matched, so if you search for an id of "ha", it will match "items.hat".
		/// </para>
		/// </summary>
		/// <param name="text">The string version of the query.</param>
		/// <returns>A <see cref="ContentQuery"/></returns>
		public static ContentQuery Parse(string text)
		{
			return DefaultQueryParser.Parse(text, StandardRules);
		}

		/// <summary>
		/// Check if the given <see cref="IContentObject"/> meets the tag, type, and id constraints.
		/// Tags are AND based, so the content must have ALL tags in the <see cref="TagConstraints"/> field.
		/// Types are OR based, so the content type only needs to be one of the types specified in the <see cref="TypeConstraints"/> field. This check allows subtypes of types in the <see cref="TypeConstraints"/> to be accepted.
		/// The id constraint is a substring match, so as long as the content's id contains the <see cref="DefaultQuery.IdContainsConstraint"/> , the id constraint passes.
		/// </summary>
		/// <param name="content">Some <see cref="IContentObject"/></param>
		/// <returns>true if the content passes the constraints, false otherwise.</returns>
		public virtual bool Accept(IContentObject content)
		{
			if (content == null) return false;

			return AcceptTag(content) && AcceptIdContains(content) && AcceptType(content.GetType());
		}

		/// <summary>
		/// Check if the given <see cref="IContentObject"/> meets the tag constraint.
		/// Tags are AND based, so the content must have ALL tags in the <see cref="TagConstraints"/> field.
		/// </summary>
		/// <param name="content">Some <see cref="IContentObject"/></param>
		/// <returns>true if the content passes the tag constraint, false otherse. </returns>
		public bool AcceptTag(IContentObject content)
		{
			if (TagConstraints == null) return true;
			if (content == null)
			{
				return TagConstraints.Count == 0;
			}

			return AcceptTags(new HashSet<string>(content.Tags));
		}

		/// <summary>
		/// Check if the given set of tags would meet the tag constraint.
		/// Tags are AND based, so the given set of tags must have ALL tags in the <see cref="TagConstraints"/> field.
		/// </summary>
		/// <param name="tags">A set of tags</param>
		/// <returns>true if the set of tags passes the tag constraint, false otherwise</returns>
		public bool AcceptTags(HashSet<string> tags)
		{
			if (TagConstraints == null) return true;
			if (tags == null) return TagConstraints.Count == 0;

			foreach (var tag in TagConstraints)
			{
				if (!tags.Contains(tag))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Check if the given generic content type passes the type constraint.
		/// Types are OR based, so the content type only needs to be one of the types specified in the <see cref="TypeConstraints"/> field.
		/// </summary>
		/// <param name="allowInherit">
		/// By default, a subclass of a type listed in the <see cref="TypeConstraints"/> set is allowed
		/// to pass the constraint. Disable this field to disable that behaviour, so that only exact type matches are accepted.
		/// </param>
		/// <typeparam name="TContent">A type of <see cref="IContentObject"/></typeparam>
		/// <returns>true if the type passes the type constraint, false otherwise.</returns>
		public bool AcceptType<TContent>(bool allowInherit = true) where TContent : IContentObject, new()
		{
			return AcceptType(typeof(TContent), allowInherit);
		}

		/// <summary>
		/// Check if the given runtime type passes the type constraint
		/// Types are OR based, so the content type only needs to be one of the types specified in the <see cref="TypeConstraints"/> field.
		/// </summary>
		/// <param name="type">A runtime type that should derive from <see cref="IContentObject"/></param>
		/// <param name="allowInherit">
		/// By default, a subclass of a type listed in the <see cref="TypeConstraints"/> set is allowed
		/// to pass the constraint. Disable this field to disable that behaviour, so that only exact type matches are accepted.
		/// </param>
		/// <returns>true if the type passes the type constraint, false otherwise.</returns>
		public bool AcceptType(Type type, bool allowInherit = true)
		{
			if (TypeConstraints == null || TypeConstraints.Count == 0) return true;

			if (type == null) return false;

			if (allowInherit)
			{
				return TypeConstraints.Any(t => t.IsAssignableFrom(type));
			}
			else
			{
				return TypeConstraints.Contains(type);
			}
		}

		/// <summary>
		/// Check if the given <see cref="IContentObject"/> content passes the id constraint.
		/// The id constraint is a substring match, so as long as the content's id contains the <see cref="DefaultQuery.IdContainsConstraint"/> , the id constraint passes.
		/// </summary>
		/// <param name="content">Some <see cref="IContentObject"/></param>
		/// <returns>rue if the content passes the id constraint, false otherwise.</returns>
		public bool AcceptIdContains(IContentObject content)
		{
			return AcceptIdContains(content?.Id);
		}

		protected static bool SerializeTagRule(ContentQuery query, out string str)
		{
			str = "";
			if (query.TagConstraints == null)
			{
				return false;
			}
			str = $"tag:{string.Join(" ", query.TagConstraints)}";
			return true;
		}

		protected static bool SerializeTypeRule(ContentQuery query, out string str)
		{
			str = "";
			if (query.TypeConstraints == null)
			{
				return false;
			}
			str = $"t:{string.Join(" ", query.TypeConstraints.Select(ContentTypeReflectionCache.Instance.TypeToName))}";
			return true;
		}

		/// <summary>
		/// Check if this <see cref="ContentQuery"/> would select the same content as the given <see cref="ContentQuery"/>.
		/// The equality match is based on set equality, not order equality.
		/// </summary>
		/// <param name="other">Some other <see cref="ContentQuery"/></param>
		/// <returns>true if the two queries select the same content, false otherwise.</returns>
		public bool EqualsContentQuery(ContentQuery other)
		{
			if (other == null) return false;

			var tagsEqual = other.TagConstraints == null || TagConstraints == null
			   ? (other.TagConstraints == null && TagConstraints == null)
			   : (other.TagConstraints.SetEquals(TagConstraints));

			var typesEqual = other.TypeConstraints == null || TypeConstraints == null
			   ? (other.TypeConstraints == null && TypeConstraints == null)
			   : other.TypeConstraints.SetEquals(TypeConstraints);

			var idEqual = (other.IdContainsConstraint?.Equals(IdContainsConstraint) ?? IdContainsConstraint == null);
			return tagsEqual &&
				   idEqual &&
				   typesEqual;
		}

		/// <summary>
		/// The equality operator. See <see cref="EqualsContentQuery(ContentQuery)"/>
		/// </summary>
		/// <param name="obj">Any object</param>
		/// <returns>true if the given object is a <see cref="ContentQuery"/> and yields true from <see cref="EqualsContentQuery"/>, false otherwise</returns>
		public override bool Equals(object obj)
		{
			return EqualsContentQuery(obj as ContentQuery);
		}

		/// <summary>
		/// Get the hashcode for the <see cref="ContentQuery"/>
		/// </summary>
		/// <returns>a hashcode</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (TypeConstraints != null ? TypeConstraints.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (TagConstraints != null ? TagConstraints.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (IdContainsConstraint != null ? IdContainsConstraint.GetHashCode() : 0);
				return hashCode;
			}
		}

		/// <summary>
		/// Convert the <see cref="ContentQuery"/> back into a string format.
		/// This method will guarantee that the output string would serialize back into a <see cref="ContentQuery"/> that would
		/// be equal when checked with the <see cref="EqualsContentQuery"/> method.
		/// </summary>
		/// <param name="existing">A string that used to represent the <see cref="ContentQuery"/>. The output string will
		/// try to conform to this format as close as possible to reduce string jitter.</param>
		/// <returns>The string form of the <see cref="ContentQuery"/></returns>
		public string ToString(string existing)
		{
			return DefaultQueryParser.ToString(existing, this, StandardSerializeRules, StandardRules);
		}

		/// <summary>
		/// Convert the <see cref="ContentQuery"/> back into a string format.
		/// This method will guarantee that the output string would serialize back into a <see cref="ContentQuery"/> that would
		/// be equal when checked with the <see cref="EqualsContentQuery"/> method.
		/// </summary>
		/// <returns>The string form of the <see cref="ContentQuery"/></returns>
		public override string ToString()
		{
			return ToString(null);
		}
	}
}
