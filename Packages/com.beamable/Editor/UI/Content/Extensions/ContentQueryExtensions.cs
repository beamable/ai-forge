using Beamable.Editor.Content.Models;

namespace Beamable.Editor.Content.Extensions
{
	public static class ContentQueryExtensions
	{
		public static bool Accepts(this EditorContentQuery query, ContentItemDescriptor descriptor)
		{
			return query.AcceptTags(descriptor.AllTags)
				   && query.AcceptIdContains(descriptor.Name)
				   && query.AcceptType(descriptor.ContentType.ContentType)
				   && query.AcceptValidation(descriptor.ValidationStatus)
				   && query.AcceptStatus(descriptor.Status);
		}
	}
}
