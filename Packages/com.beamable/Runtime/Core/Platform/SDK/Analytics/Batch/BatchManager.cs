using Beamable.Coroutines;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Api.Analytics.Batch
{

	/// <summary>
	/// Batch manager.
	/// This generic class manages the business logic which determines how and when a batch is expired.
	/// </summary>
	public class BatchManager<T>
	{
		private CoroutineService _coroutineService;

		/// <summary>
		/// Occurs when on batch expired.
		/// </summary>
		public event Action<List<T>> OnBatchExpired;

		/// <summary>
		/// Flag for whether the BatchManager is actively evaluating the batch's lifecycle
		/// </summary>
		protected bool _isActive;

		protected int _batchCapacity;
		protected double _batchTimeoutSeconds;
		protected double _heartbeatSeconds;
		protected WaitForSeconds _heartbeatInterval;
		protected IEnumerator _heatbeatCoroutine;

		protected IBatchContainer<T> _currentBatch;


		/// <summary>
		/// Initializes a new instance of the <see cref="BatchManager{T}"/> class.
		/// </summary>
		/// <param name="batchCapacity">Batch capacity threshold before expiration.</param>
		/// <param name="batchTimeoutSeconds">Batch timeout seconds before expiration.</param>
		/// <param name="heartbeatInterval">Heartbeat interval indicates how often the batch lifecycle is evaluated.</param>
		public BatchManager(CoroutineService coroutineService, int batchCapacity, double batchTimeoutSeconds, float heartbeatInterval = 1f)
		{
			_coroutineService = coroutineService;
			_isActive = false;

			_batchCapacity = batchCapacity;
			_batchTimeoutSeconds = batchTimeoutSeconds;
			SetHeartbeat(heartbeatInterval);

			RotateBatch();
		}

		/// <summary>
		/// Start this batch's lifecycle processing
		/// This starts a coroutine which executs a Heartbeat at a regular interval
		/// </summary>
		public void Start()
		{
			if (!_isActive)
			{
				// Start checking periodically
				_isActive = true;
				_heatbeatCoroutine = Heartbeat();
				_coroutineService.StartCoroutine(_heatbeatCoroutine);
				OnStart();
			}
		}

		/// <summary>
		/// Stop this batch's lifecycle processing
		/// This means the Heartbeat will cease to execute
		/// </summary>
		public void Stop()
		{
			if (_isActive)
			{
				_isActive = false;
				_coroutineService.StopCoroutine(_heatbeatCoroutine);
				_heatbeatCoroutine = null;
			}
		}

		public void RestartHeartbeat(bool onlyIfStarted = true)
		{
			if (!onlyIfStarted || _isActive)
			{
				Stop();
				Start();
			}
		}

		/// <summary>
		/// Add the specified item to the batch.
		/// </summary>
		/// <param name="item">Item.</param>
		virtual public void Add(T item)
		{
			_currentBatch.Add(item);
		}

		/// <summary>
		/// Flush this Batch
		/// This causes it to expire, trigger the relevant callbacks, and rotate
		/// </summary>
		virtual public void Flush()
		{
			_currentBatch.Expire();
		}

		/// <summary>
		/// Sets the batch's capacity threshold before expiration.
		/// </summary>
		/// <param name="batchCapacity">Batch capacity.</param>
		public void SetCapacity(int batchCapacity)
		{
			_batchCapacity = batchCapacity;
		}

		/// <summary>
		/// Sets the timeout seconds before expiration.
		/// </summary>
		/// <param name="batchTimeoutSeconds">Batch timeout seconds.</param>
		public void SetTimeoutSeconds(double batchTimeoutSeconds)
		{
			_batchTimeoutSeconds = batchTimeoutSeconds;
		}

		public void SetHeartbeat(float heatbeatInterval)
		{
			// FIXME: restarting the heartbeat has the side effect of duplicating events
			if (heatbeatInterval != _heartbeatSeconds)
			{
				_heartbeatSeconds = heatbeatInterval;
				_heartbeatInterval = Yielders.Seconds(heatbeatInterval);
				RestartHeartbeat();
			}
		}

		/// <summary>
		/// Rotates the batch.
		/// This creates a new batch and hooks the OnExpired event
		/// </summary>
		virtual protected void RotateBatch()
		{
			_currentBatch = new BatchContainer<T>(_batchCapacity, _batchTimeoutSeconds);
			_currentBatch.OnExpired += OnExpired;
		}

		/// <summary>
		/// Raises the expired event.
		/// </summary>
		/// <param name="batchItems">Batch items.</param>
		protected void OnExpired(List<T> batchItems)
		{
			RotateBatch();

			if (OnBatchExpired != null)
				OnBatchExpired(batchItems);
		}

		/// <summary>
		/// Checks whether the batch should be expired.
		/// </summary>
		virtual protected void CheckBatchExpired()
		{
			if (_currentBatch.Count > 0)
			{
				var timestampSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

				bool capacityExceeded = _currentBatch.Count >= _currentBatch.Capacity;
				bool timeoutExceeded = timestampSeconds > _currentBatch.ExpiresTimestamp;

				if (capacityExceeded || timeoutExceeded)
					_currentBatch.Expire();
			}
		}

		/// <summary>
		/// Heartbeat which occurs at a regular interval and executes lifecycle logic
		/// </summary>
		virtual protected void OnHeartbeat()
		{
			CheckBatchExpired();
		}

		/// <summary>
		/// Heartbeat coroutine
		/// Runs while the batch manager is active and yields at a regular interval
		/// </summary>
		IEnumerator Heartbeat()
		{
			while (_isActive)
			{
				OnHeartbeat();
				yield return _heartbeatInterval;
			}
		}

		virtual protected void OnStart()
		{

		}

	}
}
