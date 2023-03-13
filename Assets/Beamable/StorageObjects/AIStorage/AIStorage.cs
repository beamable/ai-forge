using Beamable.Common;
using MongoDB.Driver;

namespace Beamable.Server
{
	[StorageObject("AIStorage")]
	public class AIStorage : MongoStorageObject
	{
	}

	public static class AIStorageExtension
	{
		/// <summary>
		/// Get an authenticated MongoDB instance for AIStorage
		/// </summary>
		/// <returns></returns>
		public static Promise<IMongoDatabase> AIStorageDatabase(this IStorageObjectConnectionProvider provider)
			=> provider.GetDatabase<AIStorage>();

		/// <summary>
		/// Gets a MongoDB collection from AIStorage by the requested name, and uses the given mapping class.
		/// If you don't want to pass in a name, consider using <see cref="AIStorageCollection{TCollection}()"/>
		/// </summary>
		/// <param name="name">The name of the collection</param>
		/// <typeparam name="TCollection">The type of the mapping class</typeparam>
		/// <returns>When the promise completes, you'll have an authorized collection</returns>
		public static Promise<IMongoCollection<TCollection>> AIStorageCollection<TCollection>(
			this IStorageObjectConnectionProvider provider, string name)
			where TCollection : StorageDocument
			=> provider.GetCollection<AIStorage, TCollection>(name);

		/// <summary>
		/// Gets a MongoDB collection from AIStorage by the requested name, and uses the given mapping class.
		/// If you want to control the collection name separate from the class name, consider using <see cref="AIStorageCollection{TCollection}(string)"/>
		/// </summary>
		/// <param name="name">The name of the collection</param>
		/// <typeparam name="TCollection">The type of the mapping class</typeparam>
		/// <returns>When the promise completes, you'll have an authorized collection</returns>
		public static Promise<IMongoCollection<TCollection>> AIStorageCollection<TCollection>(
			this IStorageObjectConnectionProvider provider)
			where TCollection : StorageDocument
			=> provider.GetCollection<AIStorage, TCollection>();
	}
}
