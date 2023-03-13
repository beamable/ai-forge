using Beamable.Coroutines;
using Beamable.Pooling;
using Beamable.Serialization;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Api.Analytics.Batch
{

	/// <summary>
	/// Persistent Batch Manager.
	/// Subclass of BatchManager that adds the ability to persist batches to disk and restore from disk
	/// This BatchManager enforces that type T is a JsonSerializable.ISerializable
	/// </summary>
	public class PersistentBatchManager<T> : BatchManager<T> where T : class, JsonSerializable.ISerializable
	{

		/// <summary>
		/// The storage key used for persisting to disk.
		/// </summary>
		protected string _storageKey;

		/// <summary>
		/// A flag which determines whether to backup the current batch on the next Heartbeat.
		/// </summary>
		protected bool _backupBatchNow;

		/// <summary>
		/// Initializes a new instance of the <see cref="PersistentBatchManager{T}"/> class.
		/// </summary>
		/// <param name="storageKey">Storage key used for persistent storage.</param>
		/// <param name="batchCapacity">Batch capacity threshold for batch expiration.</param>
		/// <param name="batchTimeoutSeconds">Maximum seconds before a batch is expired.</param>
		/// <param name="heartbeatInterval">Heartbeat interval.</param>
		public PersistentBatchManager(CoroutineService coroutineService, string storageKey, int batchCapacity, double batchTimeoutSeconds, float heartbeatInterval = 1f)
			: base(coroutineService, batchCapacity, batchTimeoutSeconds, heartbeatInterval)
		{

			_storageKey = storageKey;
			_backupBatchNow = false;
		}

		override protected void OnStart()
		{
			RestoreEventBatch();
		}

		/// <summary>
		/// Add an item to the Batch
		/// </summary>
		/// <param name="item">Item.</param>
		override public void Add(T item)
		{
			base.Add(item);
			_backupBatchNow = true;
		}

		/// <summary>
		/// Rotates the batch.
		/// This creates a new batch, hooks OnExpired event, and declares the batch ready for backup
		/// </summary>
		override protected void RotateBatch()
		{
			_currentBatch = new SerializableBatch<T>(_batchCapacity, _batchTimeoutSeconds);
			_currentBatch.OnExpired += OnExpired;
			_backupBatchNow = true;
		}

		/// <summary>
		/// Raises the heartbeat event.
		/// This runs the base class OnHeartbeat and also checks whether we should backup the batch.
		/// </summary>
		override protected void OnHeartbeat()
		{
			base.OnHeartbeat();

			if (_backupBatchNow)
			{
				BackupEventBatch();
			}
		}

		/// <summary>
		/// Backups the event batch to disk.
		/// </summary>
		protected void BackupEventBatch()
		{
			try
			{
				Dictionary<string, object> batchDictionary = JsonSerializable.Serialize((SerializableBatch<T>)_currentBatch);

				using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
				{
					string batchJson = Json.Serialize(batchDictionary, pooledBuilder.Builder);
					PlayerPrefs.SetString(_storageKey, batchJson);
					PlayerPrefs.Save();
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[PersistentBatchManager] BackupEventBatch: An error occurred during saving of an analytics event batch to disk => " + e.Message);
			}

			_backupBatchNow = false;
		}

		/// <summary>
		/// Restores the event batch from disk and adds any elements to the current batch.
		/// </summary>
		protected void RestoreEventBatch()
		{
			try
			{
				string batchJson = PlayerPrefs.GetString(_storageKey);
				if (!string.IsNullOrEmpty(batchJson))
				{
					var parsedDictionary = Json.Deserialize(batchJson) as Dictionary<string, object>;

					if (parsedDictionary != null)
					{
						var eventBatch = new SerializableBatch<T>(_batchCapacity, _batchTimeoutSeconds);
						JsonSerializable.Deserialize(eventBatch, parsedDictionary);

						if (eventBatch.Items != null && eventBatch.Count > 0)
						{
							for (int i = 0; i < eventBatch.Count; ++i)
							{
								if (eventBatch.Items[i] != null)
									Add(eventBatch.Items[i]);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[PersistentBatchManager] RestoreEventBatch: An error occurred while loading an analytics event batch from disk => " + e.Message);
			}
		}
	}
}
