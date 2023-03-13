using Beamable.Serialization;

namespace Beamable.Api.Analytics.Batch
{

	/// <summary>
	/// Serializable batch.
	/// This is a specialized BatchContainer which is JSON Serializable
	/// All elements T added to it must be JSON Serializable
	/// </summary>
	public class SerializableBatch<T> : BatchContainer<T>, JsonSerializable.ISerializable
		where T : class, JsonSerializable.ISerializable
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializableBatch{T}"/> class.
		/// </summary>
		/// <param name="batchMaxSize">Batch max size.</param>
		/// <param name="batchTimeoutSeconds">Batch timeout seconds.</param>
		public SerializableBatch(int batchMaxSize, double batchTimeoutSeconds) : base(batchMaxSize, batchTimeoutSeconds) { }

		/// <summary>
		/// Serialize the batch back and forth from JSON
		/// </summary>
		/// <param name="s">S.</param>
		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			s.Serialize("expires", ref _expiresTimestamp);
			s.Serialize("capacity", ref _capacity);
			s.SerializeList("items", ref _items);
		}
	}
}
