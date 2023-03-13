using UnityEngine;

namespace Beamable.Modules.Generics
{
	public abstract class DataPresenter<T> : MonoBehaviour where T : class
	{
#pragma warning disable CS0649
		[SerializeField] private GameObject _loadingIndicator;
		[SerializeField] private GameObject _mainGroup;
#pragma warning restore CS0649

		protected T Data;

		private void Awake()
		{
			RefreshLoadingIndicator();
		}

		public virtual void Setup(T data, params object[] additionalParams)
		{
			Data = data;
			RefreshLoadingIndicator();
			Refresh();
		}

		public virtual void ClearData()
		{
			Data = null;
			RefreshLoadingIndicator();
		}

		protected abstract void Refresh();

		private void RefreshLoadingIndicator()
		{
			_loadingIndicator.SetActive(Data == null);
			_mainGroup.SetActive(Data != null);
		}
	}
}
