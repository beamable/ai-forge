using Beamable.UI.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Tournaments
{
	public class TournamentInfoBehaviour : MonoBehaviour
	{
		public TextReference Title, Body;
		public Button ReadMoreButton;

		private TournamentInfoPageSection _infoPageSection;
		private TournamentsBehaviour _root;

		public void Set(TournamentsBehaviour root, TournamentInfoPageSection infoPageSection)
		{
			_root = root;
			_infoPageSection = infoPageSection;
			Title.Value = infoPageSection.Title;
			Body.Value = infoPageSection.Body;

			if (_infoPageSection.DetailPrefab == null)
			{
				ReadMoreButton.gameObject.SetActive(false);
			}
		}

		public void ShowDetail()
		{
			_root.ShowInfoDetailPage(_infoPageSection);
		}
	}
}
