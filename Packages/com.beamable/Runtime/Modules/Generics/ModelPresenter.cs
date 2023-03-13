using UnityEngine;

namespace Beamable.Modules.Generics
{
	public abstract class ModelPresenter<T> : MonoBehaviour where T : Model, new()
	{
		protected T Model;

		protected virtual void Awake()
		{
			Model = new T();
			Model.OnRefreshRequested = RefreshRequested;
			Model.OnRefresh = Refresh;
		}

		protected virtual void OnDestroy()
		{
			Model.OnRefresh = null;
			Model.OnRefreshRequested = null;
		}

		protected abstract void RefreshRequested();
		protected abstract void Refresh();
	}
}
