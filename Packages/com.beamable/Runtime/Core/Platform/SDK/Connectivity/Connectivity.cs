using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Config;
using Beamable.Coroutines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Beamable.Api.Connectivity
{
	/// <summary>
	/// This type defines the %Client main entry point for the %Connectivity feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/connectivity-feature">Connectivity</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface IConnectivityService
	{
		/// <summary>
		/// true when Beamable thinks the current device has an internet connection.
		/// This property will update automatically as the device goes on and off line.
		/// You can use the <see cref="OnConnectivityChanged"/> event to listen for changes in this property.
		/// </summary>
		bool HasConnectivity { get; }

		/// <summary>
		/// This property allows you to simulate network outage.
		/// When true, the <see cref="Disabled"/> property will be triggered, which forces the <see cref="HasConnectivity"/> to be false, even if the device is online.
		/// When false, it is still possible that <see cref="Disabled"/> can be triggered,
		/// if the <see cref="IConnectivityServiceExtensions.GlobalForceDisabled"/> property has been set.
		/// </summary>
		bool ForceDisabled
		{
			get;
			set;
		}

		/// <summary>
		/// true if the <see cref="ForceDisabled"/> property has been set to true,
		/// OR if the <see cref="IConnectivityServiceExtensions.GlobalForceDisabled"/> property has been set to true.
		/// When <see cref="Disabled"/> is true, the <see cref="HasConnectivity"/> property will be forced to false.
		/// This can be helpful to simulate network outage, even if the device is online.
		/// </summary>
		bool Disabled
		{
			get;
		}

		/// <summary>
		/// An event that will trigger anytime the <see cref="HasConnectivity"/> status changes.
		/// </summary>
		event Action<bool> OnConnectivityChanged;

		/// <summary>
		/// Force the value of <see cref="HasConnectivity"/>.
		/// The connectivity is checked periodically, so if the given <see cref="hasInternet"/> value is not correct,
		/// the connectivity will be updated soon.
		/// If the given <see cref="hasInternet"/> value is different than the current <see cref="HasConnectivity"/> property,
		/// then the <see cref="OnConnectivityChanged"/> event will trigger as part of the execution of this method.
		/// </summary>
		/// <param name="hasInternet">true if the device has internet, false otherwise.</param>
		/// <returns>
		/// A <see cref="Promise"/> representing the completion of the operation.
		/// The promise will not complete until all promises registered with the <see cref="OnReconnectOnce(ConnectionCallback,int"/> method
		/// have completed.
		/// </returns>
		Promise SetHasInternet(bool hasInternet);

		/// <summary>
		/// Force the value of <see cref="HasConnectivity"/> to be false.
		/// The connectivity is checked periodically, so if internet connectivity returns, the <see cref="HasConnectivity"/> will be updated soon.
		/// If the value of <see cref="HasConnectivity"/> used to be true,
		/// then the <see cref="OnConnectivityChanged"/> event will trigger as part of the execution of this method.
		/// </summary>
		/// <returns>A <see cref="Promise"/> representing the completion of the operation.</returns>
		Promise ReportInternetLoss();

		/// <summary>
		/// Run an action when the <see cref="HasConnectivity"/> value returns to true.
		/// If the <see cref="HasConnectivity"/> is already true, the action will be evaluated immediately.
		/// After connectivity is restored, and the action is executed, the action will never be executed again, even
		/// if further internet drops and restorations occur.
		/// See also the <see cref="OnReconnectOnce(ConnectionCallback,int"/> method.
		/// </summary>
		/// <param name="onReconnection">Some action to invoke when internet connectivity returns.</param>
		void OnReconnectOnce(Action onReconnection);

		/// <summary>
		/// Enqueue a <see cref="Promise"/> to run when the <see cref="HasConnectivity"/> value returns to true.
		/// If the <see cref="HasConnectivity"/> is already true, the promise will be evaluated immediately.
		/// When connectivity is restored, all of the <see cref="ConnectionCallback"/>'s will be invoked,
		/// and the <see cref="Promise"/>s that are created will be awaited <i>before</i> the <see cref="SetHasInternet"/>'s return <see cref="Promise"/> is completed.
		/// </summary>
		/// <param name="promise">Some function that generates a <see cref="Promise"/>. The generator will be evaluated when internet is restored.</param>
		/// <param name="order">The order to invoke the <see cref="promise"/></param>
		void OnReconnectOnce(ConnectionCallback promise, int order = 0);
	}

	public delegate Promise ConnectionCallback();

	public static class IConnectivityServiceExtensions
	{
		private static bool _globalForceDisabled;
		public static bool GlobalForceDisabled
		{
			get => _globalForceDisabled;
			set => _globalForceDisabled = value;
		}

		public static void SetGlobalEnabled(this IConnectivityService _, bool forceDisabled) =>
			GlobalForceDisabled = forceDisabled;

		public static void ToggleGlobalEnabled(this IConnectivityService _) =>
			GlobalForceDisabled = !GlobalForceDisabled;

		public static bool GetGlobalEnabled(this IConnectivityService _) => _globalForceDisabled;
	}

	/// <summary>
	/// This type defines the %Client main entry point for the %Connectivity feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/connectivity-feature">Connectivity</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class ConnectivityService : IConnectivityService
	{
		private bool _isConnected = true;
		public bool HasConnectivity => _isConnected && !Disabled;

		private bool _forceDisabled;
		public bool ForceDisabled
		{
			get => _forceDisabled;
			set => _forceDisabled = value;
		}

		public bool Disabled => _forceDisabled || IConnectivityServiceExtensions.GlobalForceDisabled;

		private bool _first = true;

		public event Action<bool> OnConnectivityChanged;

		private event Action OnReconnection;


		public async Promise SetHasInternet(bool hasInternet)
		{
			if (Disabled)
			{
				hasInternet = false;
			}

			var isReconnection = (hasInternet && !_isConnected);
			var isChange = hasInternet != _isConnected;


			_isConnected = hasInternet;
			if (isReconnection)
			{
				_reconnectionPromises.Sort((a, b) => a.Item2.CompareTo(b.Item2));
				foreach (var reconnection in _reconnectionPromises.ToList())
				{
					await reconnection.Item1();
					_reconnectionPromises.Remove(reconnection);
				}

				// we have the tubes! Invoke any pending actions and reset it.
				OnReconnection?.Invoke();
				OnReconnection = null;
			}

			if (isChange || _first)
			{
				_first = false;
				OnConnectivityChanged?.Invoke(hasInternet);
			}
		}

		public Promise ReportInternetLoss()
		{
			// TODO: This could expand into a loss-tolerance system where the connectivity service could allow a few failed messages before declaring internet is entirely gone.
			// but for now, just report no internet...
			return SetHasInternet(false);
		}

		private List<(ConnectionCallback, int)> _reconnectionPromises = new List<(ConnectionCallback, int)>();
		public void OnReconnectOnce(ConnectionCallback callback, int order = 0)
		{
			if (HasConnectivity)
			{
				var _ = callback();
				return;
			}

			_reconnectionPromises.Add((callback, order));

		}


		public void OnReconnectOnce(Action onReconnection)
		{
			// if the state is already in a connected sense, then run this immediately.
			if (HasConnectivity)
			{
				onReconnection?.Invoke();
				return;
			}

			// queue the action to be run when the connectivity comes back.
			OnReconnection += onReconnection;
		}

		public class PromiseYieldInstruction : CustomYieldInstruction
		{
			private readonly PromiseBase _promise;

			public PromiseYieldInstruction(PromiseBase promise)
			{
				_promise = promise;
			}

			public override bool keepWaiting => !_promise.IsCompleted;
		}
	}
}
