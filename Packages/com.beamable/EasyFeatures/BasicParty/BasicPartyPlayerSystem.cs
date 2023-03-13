namespace Beamable.EasyFeatures.BasicParty
{
	public class BasicPartyPlayerSystem : CreatePartyView.IDependencies
	{
		public int MaxPlayers { get; set; }

		public bool ValidateConfirmButton(int maxPlayers)
		{
			return maxPlayers > 0;
		}
	}
}
