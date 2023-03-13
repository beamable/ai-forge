using System;

namespace Beamable.Modules.Generics
{
	public abstract class Model
	{
		public Action OnRefreshRequested;
		public Action OnRefresh;

		protected bool IsBusy { get; set; } = true;

		public abstract void Initialize(params object[] initParams);

		protected void InvokeRefreshRequested()
		{
			IsBusy = true;
			OnRefreshRequested?.Invoke();
		}

		protected void InvokeRefresh()
		{
			IsBusy = false;
			OnRefresh?.Invoke();
		}
	}
}
