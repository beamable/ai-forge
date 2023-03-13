using TMPro;
using UnityEngine;

namespace Beamable.Announcements
{
	public class AnnouncementSummary : MonoBehaviour
	{
#pragma warning disable CS0649
		[SerializeField] private TextMeshProUGUI _txtTitle;
		[SerializeField] private TextMeshProUGUI _txtBody;
#pragma warning restore CS0649

		public void Setup(string title, string body)
		{
			_txtTitle.text = title;
			_txtBody.text = body;
		}
	}
}
