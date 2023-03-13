using Beamable.Editor.Content.Models;
using System.Collections.Generic;
using System.Linq;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants.Features.ContentManager.Reset;

namespace Beamable.Editor.Content.Components
{
	public class ResetContentVisualElement : DownloadContentVisualElement
	{
		public ContentDataModel DataModel { get; set; }

		public ResetContentVisualElement() : base(nameof(ResetContentVisualElement))
		{

		}

		protected override List<ContentDownloadEntryDescriptor> GetDeleteSource(DownloadSummary summary)
		{
			List<ContentDownloadEntryDescriptor> tmp = base.GetDeleteSource(summary);

			var entries = DataModel.GetAllContents()?.Where(c => c.Status == ContentModificationStatus.LOCAL_ONLY).ToList();

			foreach (var toClear in entries)
			{
				if (DataModel.GetDescriptorForId(toClear.Id, out var desc))
				{
					var data = new ContentDownloadEntryDescriptor
					{
						AssetPath = desc.AssetPath,
						ContentId = toClear.Id,
						Tags = desc.ServerTags?.ToArray(),
						Operation = "delete",
						Uri = ""
					};
					tmp.Add(data);
				}
			}

			return tmp;
		}

		protected override void SetMessageLabel()
		{
			_messageLabel = Root.Q<Label>("message");
			_messageLabel.text = RESET_CONTENT_MESSAGE_PREVIEW;
			_messageLabel.AddTextWrapStyle();
		}

		protected override void SetDownloadSuccessMessageLabel()
		{
			_messageLabel.text = RESET_CONTENT_COMPLETE_MESSAGE;
		}

		protected override void OnDownloadSuccess()
		{
			DataModel.DeleteLocalOnlyItems();
			base.OnDownloadSuccess();
		}
	}
}

