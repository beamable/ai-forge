using Beamable.Common.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Common.Player
{
	public interface IObservable
	{
		/// <summary>
		/// An event that happens whenever the object has changed.
		/// </summary>
		event Action OnUpdated;
	}

	public interface IRefreshable
	{
		/// <summary>
		/// Forces a check of the data to make sure its up to date.
		/// <para>
		/// If a refresh is already running, then this resulting <see cref="Promise"/> will represent the existing refresh call.
		/// This means you can't have multiple refreshes happening at once.
		/// </para>
		/// <para>
		/// If there is not already a refresh happening, then this will always trigger a <see cref="OnLoadingStarted"/> and a <see cref="OnLoadingFinished"/>.
		/// If the data actually changes, then a <see cref="IObservable.OnUpdated"/> will trigger.
		/// </para>
		/// </summary>
		Promise Refresh();
	}

	public class ObservableChangeEvent<T1, T2> where T1 : Enum
	{
		public T1 Event;
		public T2 Data;
	}

	public class DefaultObservable : IObservable
	{
		/// <summary>
		/// <inheritdoc cref="IObservable.OnUpdated"/>
		/// A "change" only happens when the value of the <see cref="GetBroadcastChecksum"/> changes.
		/// </summary>
		public event Action OnUpdated;

		private int _lastBroadcastChecksum;

		protected void TriggerUpdate()
		{
			// is the data the same as it was before?
			// we make that decision based on the hash code of the element...
			var hash = GetBroadcastChecksum();
			var isDifferent = hash != _lastBroadcastChecksum;
			// var oldLast = _lastBroadcastChecksum;
			_lastBroadcastChecksum = hash;

			if (isDifferent)
			{
				OnUpdated?.Invoke();
			}
		}

		/// <summary>
		/// In case when some object looks for some additional data after <see cref="OnUpdated"/> was called this data
		/// can be reseted in overriden method 
		/// </summary>
		protected virtual void ResetChangeData()
		{ }

		/// <summary>
		/// The broadcast checksum is a concept for change-detection.
		/// <para>
		/// An observable may have a complex data structure, and detecting changes can be very context specific.
		/// This method provides a way to compute some "hash" or "checksum" of the data.
		/// The "same" internal values should always produce the same output integer.
		/// </para>
		/// <para>
		/// By default, the broadcast checksum will use the object's <see cref="GetHashCode()"/> implementation.
		/// </para>
		/// </summary>
		/// <returns>an checksum of the object state</returns>
		public virtual int GetBroadcastChecksum()
		{
			return GetHashCode();
		}
	}

	public abstract class AbsRefreshableObservable : DefaultObservable, IRefreshable
	{
		/// <summary>
		/// An event that happens when the <see cref="Refresh"/> method starts.
		/// This event will always be accompanied by a <see cref="OnLoadingFinished"/> event.
		/// <para>
		/// Between the two loading events, the <see cref="DefaultObservable.OnUpdated"/> might happen.
		/// However, if the refresh operation doesn't produce any meaningful changes as per the <see cref="DefaultObservable.GetBroadcastChecksum"/>
		/// Then the <see cref="DefaultObservable.OnUpdated"/> event <i>won't</i> happen.
		/// </para>
		/// </summary>
		public event Action OnLoadingStarted;

		/// <summary>
		/// An event that happens after the <see cref="Refresh"/> method finishes.
		/// This event will always be after a <see cref="OnLoadingStarted"/> event.
		/// <para>
		/// Between the two loading events, the <see cref="DefaultObservable.OnUpdated"/> might happen.
		/// However, if the refresh operation doesn't produce any meaningful changes as per the <see cref="DefaultObservable.GetBroadcastChecksum"/>
		/// Then the <see cref="DefaultObservable.OnUpdated"/> event <i>won't</i> happen.
		/// </para>
		/// </summary>
		public event Action OnLoadingFinished;

		private Promise _pendingRefresh;

		/// <summary>
		/// Represents if the object is between <see cref="OnLoadingStarted"/> and <see cref="OnLoadingFinished"/> events.
		/// </summary>
		public bool IsLoading
		{
			get;
			private set;
		}

		/// <inheritdoc cref="IRefreshable.Refresh"/>
		public async Promise Refresh()
		{
			if (IsLoading)
			{
				await _pendingRefresh;
				return;
			}

			IsLoading = true;
			try
			{
				OnLoadingStarted?.Invoke();
				_pendingRefresh = PerformRefresh();
				await _pendingRefresh;
				TriggerUpdate();
			}
			// TODO: error case?
			finally
			{
				_pendingRefresh = null;
				IsLoading = false;
				OnLoadingFinished?.Invoke();
				ResetChangeData();
			}
		}

		protected abstract Promise PerformRefresh();
	}

	[Serializable]
	public class ObservableLong : Observable<long>
	{
		public static implicit operator ObservableLong(long data) => new ObservableLong { Value = data };
	}

	[Serializable]
	public class ObservableString : Observable<string>
	{
		public static implicit operator ObservableString(string data) => new ObservableString { Value = data };
	}

	public class Observable<T> : AbsRefreshableObservable
	{
		[SerializeField]
		private T _data = default(T);

		private bool _assigned;

		public bool IsAssigned => _assigned;

		public bool IsNullOrUnassigned => !_assigned || _data == null;

		public event Action<T> OnDataUpdated;

		public virtual T Value
		{
			get => _data;
			set
			{
				_assigned = true;
				_data = value;
				TriggerUpdate();
			}
		}

		public static implicit operator T(Observable<T> observable) => observable.Value;

		public Observable()
		{
			OnUpdated += () =>
			{
				OnDataUpdated?.Invoke(_data);
			};
		}

		public Observable(T data) : this()
		{
			_data = data;
		}

		public void BindTo(Observable<T> other)
		{
			other.OnUpdated += () => Value = other.Value;
		}

		public override string ToString()
		{
			return _data?.ToString();
		}

		protected override Promise PerformRefresh()
		{
			// do nothing.
			return Promise.Success;
		}

		// public override object GetData() => Value;

		public override int GetBroadcastChecksum()
		{
			return _assigned ? _data?.GetHashCode() ?? 0 : -1;
		}
	}

	public interface IObservableReadonlyList<out T> : IReadOnlyCollection<T>, IObservable
	{
		T this[int index]
		{
			get;
		}
	}

	public interface IObservableReadonlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IObservable { }

	public interface IGetProtectedDataList<T>
	{
		List<T> Data
		{
			get;
		}

		void CheckForUpdate();
	}

	public abstract class AbsObservableReadonlyList<T> : AbsRefreshableObservable, IObservableReadonlyList<T>,
														 IGetProtectedDataList<T>
	{
		[SerializeField]
		private List<T> _data = new List<T>(); // set to new() to avoid null

		public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public int Count => _data.Count;
		public T this[int index] => _data[index];


		/// <summary>
		/// An event that happens right after <see cref="IObservable.OnUpdated"/>, but this one contains the list of internal data.
		/// </summary>
		public event Action<List<T>> OnDataUpdated;

		/// <summary>
		/// An event that happens when new items are added to the list from a <see cref="AbsRefreshableObservable.Refresh"/> call.
		/// This event happens after the <see cref="IObservable.OnUpdated"/> event
		/// </summary>
		public event Action<IEnumerable<T>> OnElementsAdded;

		/// <summary>
		/// An event that happens when new items are removed to the list from a <see cref="AbsRefreshableObservable.Refresh"/> call.
		/// This event happens after the <see cref="IObservable.OnUpdated"/> event
		/// </summary>
		public event Action<IEnumerable<T>> OnElementRemoved;

		public bool IsInitialized
		{
			get;
			protected set;
		}

		public AbsObservableReadonlyList()
		{
			OnUpdated += () =>
			{
				OnDataUpdated?.Invoke(_data.ToList());
			};
		}

		public override int GetBroadcastChecksum()
		{
			/*
			 * We want to use a hash code based on the elements of the list at the given moment.
			 */
			var res = 0x2D2816FE;
			foreach (var item in this)
			{
				var itemCode = 0;
				switch (item)
				{
					case DefaultObservable obs:
						itemCode = obs.GetBroadcastChecksum();
						break;
					case object obj:
						itemCode = obj.GetHashCode();
						break;
				}
				res = res * 31 + (itemCode);
			}

			return res;
		}

		protected void SetData(List<T> nextData)
		{
			// check for additions, and deletions...
			var added = new HashSet<T>();
			var existing = new HashSet<T>(_data);
			foreach (var next in nextData)
			{
				if (!existing.Contains(next))
				{
					added.Add(next);
				}
				else
				{
					existing.Remove(next);
				}
			}

			_data = nextData;

			if (existing.Count > 0)
			{
				OnElementRemoved?.Invoke(existing);
			}

			if (added.Count > 0)
			{
				OnElementsAdded?.Invoke(added);
			}
			TriggerUpdate();
		}

		// public override object GetData() => _data;
		List<T> IGetProtectedDataList<T>.Data => _data;
		void IGetProtectedDataList<T>.CheckForUpdate() => TriggerUpdate();
	}

	public abstract class AbsObservableReadonlyDictionary<TValue, TDict>
		: AbsRefreshableObservable, IObservableReadonlyDictionary<string, TValue>
		where TDict : SerializableDictionaryStringToSomething<TValue>, new()
	{

		[SerializeField]
		private TDict _data =
			new TDict();

		/// <summary>
		/// An event that happens right after <see cref="IObservable.OnUpdated"/>, but this one contains the dictionary of internal data.
		/// </summary>
		public event Action<SerializableDictionaryStringToSomething<TValue>> OnDataUpdated;


		public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator() => _data.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

		public int Count => _data.Count;

		public bool ContainsKey(string key) => _data.ContainsKey(key);

		public bool TryGetValue(string key, out TValue value) => _data.TryGetValue(key, out value);

		public TValue this[string key] => _data[key];

		public IEnumerable<string> Keys => _data.Keys;
		public IEnumerable<TValue> Values => _data.Values;

		// public override object GetData() => _data;

		public bool IsInitialized
		{
			get;
			protected set;
		}

		public AbsObservableReadonlyDictionary()
		{
			OnUpdated += () =>
			{
				OnDataUpdated?.Invoke(_data);
			};
		}

		protected void SetData(TDict nextData)
		{
			_data = nextData;
		}

		public override int GetBroadcastChecksum()
		{
			/*
			  * We want to use a hash code based on the elements of the list at the given moment.
			  */
			var res = 0x2D2816FE;
			foreach (var item in this)
			{
				res = res * 31 + (item.Value == null ? 0 : item.GetHashCode());
			}

			// TODO: need to include keys in hash.
			return res;
		}
	}

	public class ObservableReadonlyList<T> : AbsObservableReadonlyList<T>
	{
		private readonly Func<Promise<List<T>>> _refresh;

		public ObservableReadonlyList(Func<Promise<List<T>>> refreshFunction)
		{
			_refresh = refreshFunction;
		}

		protected override async Promise PerformRefresh()
		{
			if (_refresh != null)
			{
				var list = await _refresh.Invoke();
				SetData(list);
			}
		}
	}
}
