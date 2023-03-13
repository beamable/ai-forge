using Beamable.Common.Dependencies;
using System;
using UnityEngine;

namespace Beamable
{
	public interface IGameObjectContext
	{
		GameObject GameObject { get; }
	}

	public static class GameObjectContextExtensions
	{
		public static IDependencyBuilder AddComponentSingleton<T>(this IDependencyBuilder builder)
			where T : Component
		{
			return builder.AddComponentSingleton<T>((c, p) => { });
		}

		public static IDependencyBuilder AddComponentSingleton<T>(this IDependencyBuilder builder, Action<T, IDependencyProvider> initializer)
			where T : Component
		{
			builder.AddSingleton(provider =>
			{
				var gob = provider.GetService<IGameObjectContext>().GameObject;
				var existing = gob.GetComponent<T>();
				if (existing != null) return existing;

				existing = gob.AddComponent<T>();
				initializer(existing, provider);
				return existing;
			});
			return builder;
		}
	}
}
