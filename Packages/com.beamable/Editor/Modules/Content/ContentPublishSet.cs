using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Editor.Content
{
	public class ContentPublishSet
	{
		public string ManifestId;
		public Manifest ServerManifest;

		public List<ContentObject> ToAdd, ToModify;
		public List<string> ToDelete;

		public int totalOpsCount => ToAdd.Count + ToDelete.Count + ToModify.Count;

		public bool HasValidationErrors(IValidationContext ctx, out List<string> errors)
		{
			errors = new List<string>();

			var allContent = ToAdd.Concat(ToModify);
			foreach (var content in allContent)
			{
				if (content.HasValidationErrors(ctx, out var addErrors))
				{
					errors.AddRange(addErrors.Select(err => $"[{content.Id}] {err}"));
				}
			}

			return errors.Count > 0;
		}
	}

	public struct PublishProgress
	{
		public int TotalOperations, CompletedOperations;

		public float Progress => CompletedOperations / (float)TotalOperations;
	}
}
