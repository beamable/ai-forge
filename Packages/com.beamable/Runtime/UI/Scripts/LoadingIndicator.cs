using Beamable;
using Beamable.AccountManagement;
using Beamable.Coroutines;
using Beamable.UI.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingIndicator : MonoBehaviour
{
	public TextReference LoadingText;
	public Image LoadingImage;
	public bool StartOff;
	public float FlashProtectionSeconds = .1f;

	private bool _showEverRequested = false;

	private Queue<LoadingArg> _sessionQueue = new Queue<LoadingArg>();
	private LoadingArg _currentSession;

	private bool _needsToShow;

	public void Show(LoadingArg loadingSession)
	{
		if (loadingSession == null || loadingSession.Promise.IsCompleted)
		{
			// the loading has already completed by the time the event was triggered.
			return;
		}

		_showEverRequested = true;

		if (_currentSession == null)
		{
			//ServiceManager.Resolve<CoroutineService>().StartCoroutine(ShowAfterFlashProtection());
			BeamContext.Default.ServiceProvider.GetService<CoroutineService>()
					   .StartCoroutine(ShowAfterFlashProtection());

			loadingSession.Promise.Then(_ => { Hide(); });
			_currentSession = loadingSession;
		}
		else
		{
			_sessionQueue.Enqueue(loadingSession);
		}
	}

	public void Hide()
	{
		_currentSession = null;

		if (_sessionQueue.Count == 0)
		{
			LoadingText.gameObject.SetActive(false);
			LoadingImage.gameObject.SetActive(false);
			gameObject.SetActive(false);
		}
		else
		{
			_currentSession = _sessionQueue.Dequeue();
			if (LoadingText != null)
			{
				LoadingText.Value = _currentSession.Message;
			}
			_currentSession.Promise.Then(_ => { Hide(); });
		}

	}

	public void Show(string message)
	{
		LoadingText.gameObject.SetActive(true);
		LoadingImage.gameObject.SetActive(true);
		gameObject.SetActive(true);
		LoadingText.Value = message;
	}

	private void Start()
	{
		if (_showEverRequested) return;

		if (StartOff)
		{
			Hide();
		}
		else
		{
			Show(LoadingText.Value);
		}
	}

	private void OnEnable()
	{
		if (StartOff)
		{
			Hide();
		}
		else
		{
			Show(LoadingText.Value);
		}
	}

	IEnumerator ShowAfterFlashProtection()
	{
		yield return Yielders.Seconds(FlashProtectionSeconds);
		if (_currentSession != null)
		{
			Show(_currentSession.Message);
		}
	}
}
