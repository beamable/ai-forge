using Beamable.Common;
using Core.Platform;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Beamable.UI.Scripts
{

	public static class AsyncOperationHandleExtensions
	{
		public static AwaitableAsyncOperationHandle<T> GetAwaiter<T>(this AsyncOperationHandle<T> self)
		{
			return new AwaitableAsyncOperationHandle<T>(self);
		}
	}

	public class AwaitableAsyncOperationHandle<T> : BeamableTaskLike<T>
	{
		private readonly AsyncOperationHandle<T> _handle;
		public AwaitableAsyncOperationHandle(AsyncOperationHandle<T> handle)
		{
			_handle = handle;

		}

		public override T GetResult() => _handle.Result;
		public override bool IsCompleted => _handle.IsDone;

		public override void UnsafeOnCompleted(Action continuation)
		{
			_handle.Completed += handle => continuation();
		}
	}



	/// <summary>
	/// Helper class for loading sprites and sprite textures from Addressable
	/// Assets.
	/// </summary>
	public static class AddressableSpriteLoader
	{
		/// <summary>
		/// Given a sprite asset reference, load the sprite itself.
		/// </summary>
		/// <param name="reference">The addressable sprite.</param>
		/// <returns>The sprite that was loaded.</returns>
		public static Promise<Sprite> LoadSprite(this AssetReferenceSprite reference)
		{
			// OperationHandle will be only be valid after the first load, but if
			// it is valid we MUST use it instead of LoadAssetAsync.

			if (!reference.RuntimeKeyIsValid())
			{
				return default; // there is no asset.
			}

			var handle = reference.OperationHandle.IsValid()
			   ? reference.OperationHandle.Convert<Sprite>()
			   : reference.LoadAssetAsync();
			return SpriteFromHandle(handle);
		}

		/// <summary>
		/// Given the string path to an addressable sprite, load the sprite itself.
		/// </summary>
		/// <param name="address">Addressable path to the sprite.</param>
		/// <returns>The sprite that was loaded.</returns>
		public static Promise<Sprite> LoadSprite(string address)
		{
			return SpriteFromHandle(Addressables.LoadAssetAsync<Sprite>(address));
		}

		/// <summary>
		/// Given an AsyncOperationHandle from an Addressable Assets loading
		/// operation, get the sprite that has been loaded or will be loaded.
		/// </summary>
		/// <param name="handle">The asynchronous operation handle.</param>
		/// <returns>The sprite, once loaded.</returns>
		public static Promise<Sprite> SpriteFromHandle(AsyncOperationHandle<Sprite> handle)
		{
			var awaiter = handle.GetAwaiter().ToPromise();
			return awaiter;
		}

		/// <summary>
		/// Given a sprite asset reference, fetch its texture.
		/// If the given <see cref="AssetReferenceSprite"/> isn't valid or doesn't exist, the resulting texture will be null.
		/// </summary>
		/// <param name="reference">The addressable sprite.</param>
		/// <returns>The 2D texture of that sprite.</returns>
		public static Promise<Texture2D> LoadTexture(this AssetReferenceSprite reference)
		{
			if (!reference.RuntimeKeyIsValid())
				return Promise<Texture2D>.Successful(null);

			var sprite = LoadSprite(reference);
			return sprite?.Map(s =>
		   {
			   var isDone = sprite.IsCompleted;
			   return s.texture;
		   });
		}
	}
}
