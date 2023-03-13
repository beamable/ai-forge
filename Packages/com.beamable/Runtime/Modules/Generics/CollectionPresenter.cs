using UnityEngine;

namespace Beamable.Modules.Generics
{
	public abstract class CollectionPresenter<T> : MonoBehaviour where T : class
	{
		protected T Collection { get; set; }
	}
}
