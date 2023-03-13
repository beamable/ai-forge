using System;
using System.Collections.Generic;

namespace Beamable.Api.Analytics.Batch
{

	/// <summary>
	/// Batch container.
	/// This is a basic batch container.
	/// </summary>
	public class BatchContainer<T> : IBatchContainer<T>
	{

		/// <summary>
		/// Occurs when the batch expires.
		/// </summary>
		public event Action<List<T>> OnExpired;

		/// <summary>
		/// Gets a value indicating whether this batch is expired.
		/// </summary>
		/// <value>true</value>
		/// <c>false</c>
		public bool IsExpired
		{
			get;
			protected set;
		}

		protected long _expiresTimestamp;

		/// <summary>
		/// Gets the expires (unix) timestamp.
		/// </summary>
		/// <value>The expires timestamp.</value>
		public long ExpiresTimestamp
		{
			get { return _expiresTimestamp; }
			protected set { _expiresTimestamp = value; }
		}

		protected int _capacity;

		/// <summary>
		/// Gets the batch's max capacity before expiration.
		/// </summary>
		/// <value>The capacity.</value>
		public int Capacity
		{
			get { return _capacity; }
		}

		protected List<T> _items;

		/// <summary>
		/// Gets or sets the items in this batch.
		/// </summary>
		/// <value>The items.</value>
		public List<T> Items
		{
			get { return _items; }
			protected set { _items = value; }
		}

		/// <summary>
		/// Gets the count of elements in the batch.
		/// </summary>
		/// <value>The count.</value>
		public int Count
		{
			get { return _items.Count; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BatchContainer{T}"/> class.
		/// </summary>
		/// <param name="batchMaxSize">Batch max capacity threshold before expiration.</param>
		/// <param name="batchTimeoutSeconds">Batch timeout seconds before expiration.</param>
		public BatchContainer(int batchMaxSize, double batchTimeoutSeconds)
		{
			_expiresTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (long)batchTimeoutSeconds;
			_capacity = batchMaxSize;
			_items = new List<T>();
			IsExpired = false;
		}

		/// <summary>
		/// Add the specified item to the batch.
		/// </summary>
		/// <param name="item">Item.</param>
		virtual public void Add(T item)
		{
			_items.Add(item);
		}

		/// <summary>
		/// Expire this batch.
		/// </summary>
		virtual public void Expire()
		{
			IsExpired = true;

			if (OnExpired != null)
				OnExpired(Items);
		}
	}
}
