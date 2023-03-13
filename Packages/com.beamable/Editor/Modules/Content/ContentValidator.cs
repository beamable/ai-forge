using Beamable.Common;
using Beamable.Common.Content.Validation;
using Beamable.Editor.Content.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Beamable.Editor.Content
{
	public delegate void HandleValidationErrors(ContentExceptionCollection errors);

	public class ContentValidator
	{
		private readonly ContentIO _io;

		public ContentValidator(ContentIO io)
		{
			_io = io;
		}

		public Promise<List<ContentExceptionCollection>> Validate(IValidationContext ctx, int allContentCount, IEnumerable<ContentItemDescriptor> allContent, HandleContentProgress progressHandler = null, HandleValidationErrors errorHandler = null)
		{
			var promise = new Promise<List<ContentExceptionCollection>>();
			Task.Run(() =>
			{
				progressHandler?.Invoke(0, 0, allContentCount);
				var count = 0f;
				try
				{
					var validationPromises = new List<Promise<ContentExceptionCollection>>();

					foreach (var contentDescriptor in allContent)
					{
						//                  System.Threading.Thread.Sleep(50);
						if (contentDescriptor.LocalStatus != HostStatus.AVAILABLE)
							continue; // cannot validate server content. (yet?)

						var validationPromise = new Promise<ContentExceptionCollection>();
						validationPromises.Add(validationPromise);

						var localContent = contentDescriptor.GetLocalContent();
						EditorApplication.delayCall += () =>
					 {
						 var localObject = _io.LoadContent(contentDescriptor.AssetPath);

						 ContentExceptionCollection collection = null;
						 if (localObject.HasValidationExceptions(ctx, out var exceptions))
						 {
							 contentDescriptor.EnrichWithValidationErrors(exceptions);
							 collection = new ContentExceptionCollection(localObject, exceptions);
							 errorHandler?.Invoke(collection);
						 }
						 else
						 {
							 contentDescriptor.EnrichWithNoValidationErrors();
						 }

						 count += 1;
						 var progress = count / allContentCount;
						 progressHandler?.Invoke(progress, (int)count, allContentCount);

						 validationPromise.CompleteSuccess(collection);

					 };
					}

					Promise.Sequence(validationPromises).Then(allValidationErrors =>
				 {
					 EditorApplication.delayCall += () =>
				  {

					  progressHandler?.Invoke(1, allContentCount, allContentCount);
					  promise.CompleteSuccess(allValidationErrors.Where(e => e != null).ToList());
				  };
				 });

				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					throw ex;
				}
			});

			return promise;
		}
	}
}
