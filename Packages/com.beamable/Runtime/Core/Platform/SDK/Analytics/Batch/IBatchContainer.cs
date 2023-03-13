using System;
using System.Collections.Generic;

namespace Beamable.Api.Analytics.Batch
{

	/// <summary>
	/// Batch Container Interface
	/// Batch containers encapsulate the data of a batch
	/// </summary>
	public interface IBatchContainer<T>
	{

		/// <summary>
		/// Occurs when the batch expires.
		/// </summary>
		event Action<List<T>> OnExpired;

		/// <summary>
		/// Gets a value indicating whether this batch is expired.
		/// </summary>
		/// <value><c>true</c> if this instance is expired; otherwise, <c>false</c>.</value>
		bool IsExpired
		{
			get;
		}

		/// <summary>
		/// Gets the expires (unix) timestamp.
		/// </summary>
		/// <value>The expires timestamp.</value>
		long ExpiresTimestamp
		{
			get;
		}

		/// <summary>
		/// Gets the batch's max capacity before expiration.
		/// </summary>
		/// <value>The capacity.</value>
		int Capacity
		{
			get;
		}

		/// <summary>
		/// Gets the count of elements in the batch.
		/// </summary>
		/// <value>The count.</value>
		int Count
		{
			get;
		}

		/// <summary>
		/// Add the specified item to the batch.
		/// </summary>
		/// <param name="item">Item.</param>
		void Add(T item);

		/// <summary>
		/// Expire this batch.
		/// </summary>
		void Expire();
	}
}
