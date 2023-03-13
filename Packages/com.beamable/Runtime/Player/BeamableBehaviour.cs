using Beamable.Common.Content;
using UnityEngine;

namespace Beamable
{
	public enum BeamableBehaviourBootstrapType
	{
		NONE,
		ON_START,
		ON_UPDATE
	}

	/// <summary>
	/// This component is a Unity component hook to access a <see cref="BeamContext"/> object.
	/// Every <see cref="BeamableBehaviour"/> has one <see cref="BeamContext"/>. The gameobject that holds this component may
	/// also be used by Beamable to store other required components for the <see cref="BeamContext"/>.
	///
	/// <para>
	/// When this component is destroyed, if it is the primary responsible <see cref="BeamableBehaviour"/> for the <see cref="BeamContext"/>, then
	/// it may dispose the <see cref="BeamContext"/> as well.
	/// </para>
	/// </summary>
	[DisallowMultipleComponent]
	public class BeamableBehaviour : MonoBehaviour
	{
		public OptionalString prefix = new OptionalString();

		/// <summary>
		/// By default, when this component is destroyed, if it is the primary owner of the <see cref="BeamContext"/>, then the context will be disposed.
		/// However, if the <see cref="DontDestroyContext"/> is true, then the context will not be disposed when the behaviour is destroyed. The <see cref="BeamContext"/> has gameObject internal dependencies,
		/// so in order to keep it alive, this gameObject must be marked as DontDestroyOnLoad. If you mark the <see cref="DontDestroyContext"/> property, then the gameobject will be moved to root level, and marked as DontDestroyOnLoad
		/// <para>
		/// The <see cref="BeamContext.Default"/> context will have <see cref="DontDestroyContext"/> set to true by default so that the context persists between scene loads.
		/// </para>
		/// </summary>
		public OptionalBoolean DontDestroyContext = new OptionalBoolean();

		[SerializeField]
		private BeamContext _context;

		[SerializeField]
		private BeamableBehaviour _contextBehaviour;

		public BeamableBehaviourBootstrapType BootstrapType = BeamableBehaviourBootstrapType.ON_START;

		/// <summary>
		/// Returns true if there is an initialized <see cref="BeamContext"/> associated with this behaviour yet.
		/// </summary>
		public bool HasContext => _context?.IsInitialized ?? false;

		/// <summary>
		/// Returns true if this behaviour is sitting on the gameObject that is housing the other required components for a <see cref="BeamContext"/>
		/// </summary>
		public bool IsOwner => _contextBehaviour == this;

		private bool createdFirstContext;

		// Lazy access
		/// <summary>
		/// The <see cref="BeamContext"/> that is associated with this behaviour.
		/// </summary>
		public BeamContext Context =>
			HasContext
				? _context
				: (_context = BeamContext.Instantiate(this, prefix?.Value));


		private void Start()
		{
			// Exists so that the component can be disabled via the Inspector.
			CheckBootstrap(BeamableBehaviourBootstrapType.ON_START);
		}


		internal BeamableBehaviour Initialize(BeamContext ctx)
		{

			_context = ctx;
			prefix.Value = ctx.PlayerCode;
			prefix.HasValue = true;
			_contextBehaviour = ctx.ServiceProvider.GetService<BeamableBehaviour>();

			if (IsOwner && (DontDestroyContext?.HasValue ?? false) && DontDestroyContext.Value)
			{
				DontDestroyOnLoad(gameObject);
			}

			return this;
		}


		private void Update()
		{
			if (!CheckBootstrap(BeamableBehaviourBootstrapType.ON_UPDATE) && (!IsOwner && !_contextBehaviour))
			{
				_context = null;
			}
		}

		private bool CheckBootstrap(BeamableBehaviourBootstrapType type)
		{
			if (BootstrapType == type && !HasContext && !createdFirstContext)
			{
				createdFirstContext = true;
				Touch();
				return true;
			}

			return false;
		}

		private void OnDestroy()
		{
			if (!IsOwner) return;
			if (DontDestroyContext.HasValue && DontDestroyContext.Value) return;
			Dispose();
		}

		[ContextMenu(CONTEXT_MENU_DISPOSE, false, CONTEXT_MENU_ORDER)]
		void Dispose()
		{
			_context?.Stop();
		}

		[ContextMenu(CONTEXT_MENU_DISPOSE, true, CONTEXT_MENU_ORDER)]
		bool DisposeValidate() => _context != null && !_context.IsStopped && _context.IsInitialized;

		[ContextMenu(CONTEXT_MENU_TOUCH, false, CONTEXT_MENU_ORDER)]
		void Touch()
		{
			var _ = Context;
			_contextBehaviour = _context.ServiceProvider.GetService<BeamableBehaviour>();
		}

		[ContextMenu(CONTEXT_MENU_TOUCH, true, CONTEXT_MENU_ORDER)]
		bool TouchValidate() => _context == null || _context.IsStopped || !_context.IsInitialized;

		private const string CONTEXT_MENU_TOUCH = "Touch Beam Context";
		private const string CONTEXT_MENU_DISPOSE = "Dispose Beam Context";
		private const int CONTEXT_MENU_ORDER = 300;
	}
}
