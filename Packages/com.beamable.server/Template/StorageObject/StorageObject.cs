using Beamable.Common;
using MongoDB.Driver;

namespace Beamable.Server
{
	[StorageObject("XXXX")]
	public class XXXX : MongoStorageObject
	{
	}

	public static class XXXXExtension
	{
		/// <summary>
		/// Get an authenticated MongoDB instance for XXXX
		/// </summary>
		/// <returns></returns>
		public static Promise<IMongoDatabase> XXXXDatabase(this IStorageObjectConnectionProvider provider)
			=> provider.GetDatabase<XXXX>();

		/// <summary>
		/// Gets a MongoDB collection from XXXX by the requested name, and uses the given mapping class.
		/// If you don't want to pass in a name, consider using <see cref="XXXXCollection{TCollection}()"/>
		/// </summary>
		/// <param name="name">The name of the collection</param>
		/// <typeparam name="TCollection">The type of the mapping class</typeparam>
		/// <returns>When the promise completes, you'll have an authorized collection</returns>
		public static Promise<IMongoCollection<TCollection>> XXXXCollection<TCollection>(
			this IStorageObjectConnectionProvider provider, string name)
			where TCollection : StorageDocument
			=> provider.GetCollection<XXXX, TCollection>(name);

		/// <summary>
		/// Gets a MongoDB collection from XXXX by the requested name, and uses the given mapping class.
		/// If you want to control the collection name separate from the class name, consider using <see cref="XXXXCollection{TCollection}(string)"/>
		/// </summary>
		/// <param name="name">The name of the collection</param>
		/// <typeparam name="TCollection">The type of the mapping class</typeparam>
		/// <returns>When the promise completes, you'll have an authorized collection</returns>
		public static Promise<IMongoCollection<TCollection>> XXXXCollection<TCollection>(
			this IStorageObjectConnectionProvider provider)
			where TCollection : StorageDocument
			=> provider.GetCollection<XXXX, TCollection>();
	}
}
