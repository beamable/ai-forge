using Beamable.Common.Reflection;
using System;
using UnityEngine;

namespace Beamable.Reflection
{
	/// <summary>
	/// The base <see cref="ScriptableObject"/> type to wrap around any <see cref="IReflectionSystem"/> that a user may want to piggy-back in our assembly sweep
	/// done in <see cref="ReflectionCache"/> and <see cref="EditorAPI"/> and <see cref="Beamable.API"/> initializations.
	/// </summary>
	public abstract class ReflectionSystemObject : ScriptableObject
	{
		[Tooltip("Used to disable reflection systems from running if you are not going to use them.")]
		public bool Enabled = true;

		[Tooltip("Used to sort all reflection systems in ascending order before generating the reflection cache. Lowest value means highest priority.")]
		public int Priority;

		/// <summary>
		/// The <see cref="IReflectionSystem"/> instance. There are two common options here:
		/// <list type="bullet">
		/// <item>
		/// Implement <see cref="IReflectionSystem"/> in the scriptable object inheriting from this class and return "this" to in the implementation of this property.
		/// </item>
		/// <item>
		/// Implement <see cref="IReflectionSystem"/> in another regular old C# class.
		/// </item>
		/// </list>
		///
		/// Reasons for choosing how to do implement these are heavily dependent on what the reflection-based system is trying to do, whether or not it is a runtime system
		/// so we leave it to you to decided.
		/// </summary>
		public abstract IReflectionSystem System
		{
			get;
		}

		/// <summary>
		/// Returns the <see cref="IReflectionTypeProvider"/> that informs the <see cref="ReflectionCache"/> which types this system cares about. Usually will be the same as the
		/// <see cref="System"/> object.
		/// </summary>
		public abstract IReflectionTypeProvider TypeProvider
		{
			get;
		}

		/// <summary>
		/// The concrete type of the <see cref="System"/>.
		/// </summary>
		public abstract Type SystemType
		{
			get;
		}
	}
}
