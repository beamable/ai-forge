using System.Collections.Generic;

namespace Beamable.UI.Buss
{
	public interface IVariablesProvider
	{
		List<BussStyleSheet> GetStylesheets();
	}
}
