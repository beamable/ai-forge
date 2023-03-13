using System;
using UnityEditor;
using UnityEngine.Assertions;

namespace Beamable.Editor
{
	public static class BeamableAssetDatabase
	{
		/// <summary>
		/// A sugar on top of <see cref="AssetDatabase.FindAssets"/> that uses the type's full name,
		/// so that we don't collide with client namespaces or common types.
		/// </summary>
		/// <param name="searchInFolders">The folders where the search will start.</param>
		/// <returns>
		///   <para>Array of matching asset. Note that GUIDs will be returned.</para>
		/// </returns>
		public static string[] FindAssets(Type t, string[] searchInFolders = null)
		{
			Assert.IsNotNull(t, "Cannot find assets for null type");
			Assert.IsFalse(t.FullName != null && t.FullName.Contains(nameof(UnityEditorInternal)),
						   $"Type {t.FullName} is part of `UnityEditorInternal`- these assets should be found using just nameof, not full type name");
			var fullName = t.FullName;

			return AssetDatabase.FindAssets($"t:{fullName}", searchInFolders);
		}

		/// <inheritdoc cref="FindAssets"/>
		public static string[] FindAssets<T>(string[] searchInFolders = null) => FindAssets(typeof(T), searchInFolders);
	}
}
