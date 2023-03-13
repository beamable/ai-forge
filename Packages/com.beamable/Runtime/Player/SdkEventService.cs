using Beamable.Api.Connectivity;
using Beamable.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Player
{

	public interface ISdkEventService
	{
		/// <summary>
		/// Add a <see cref="SdkEvent"/> to the event pipeline.
		/// Any <see cref="SdkEventConsumer"/>s that were created with the <see cref="Register"/> method,
		///  that match the <see cref="evt"/>'s <see cref="SdkEvent.Source"/> property will be triggered.
		/// If there are no events being processed when this method is called, the event will be processed immediately.
		/// If there are already events being processed, or if this method is called as a side effect of an earlier call to <see cref="Add"/>,
		///  then the event will be enqueued and processed after all existing events have been processed.
		/// </summary>
		/// <param name="evt">
		/// Any <see cref="SdkEvent"/>
		/// </param>
		/// <returns>
		/// A <see cref="Promise{T}"/> that will complete when the given <see cref="evt"/>'s <see cref="SdkEventConsumer"/>'s have finished processing the event.
		/// </returns>
		Promise Add(SdkEvent evt);

		/// <summary>
		/// This will be removed in a future version of Beamable, please do not use.
		/// The processing happens immediately when events are added via the <see cref="Add"/> method.
		/// There should never be a need to manually process the events.
		/// </summary>
		void Process();

		/// <summary>
		/// Register a handler function that will trigger anytime an <see cref="SdkEvent"/> is processed that matches the given <see cref="source"/> argument.
		/// </summary>
		/// <param name="source">
		/// Specify which <see cref="SdkEvent.Source"/> value the <see cref="handler"/> will be called for.
		/// </param>
		/// <param name="handler">
		/// A function that consumes the <see cref="SdkEvent"/>, and produces a <see cref="Promise"/>.
		/// The processing of an <see cref="SdkEvent"/> won't complete until all the resulting promises complete.
		/// </param>
		/// <returns>
		/// A <see cref="SdkEventConsumer"/> that represents the registration of the <see cref="handler"/> on the <see cref="source"/>.
		/// </returns>
		SdkEventConsumer Register(string source, SdkEventHandler handler);

		/// <summary>
		/// Unregister a <see cref="SdkEventConsumer"/> so that the <see cref="SdkEventHandler"/> will not trigger for <see cref="SdkEvent"/>'s of the
		/// given <see cref="SdkEventConsumer.Source"/>.
		/// </summary>
		/// <param name="consumer">
		/// The <see cref="SdkEventConsumer"/> instance to unregister. This must be the instance that was returned from the <see cref="Register"/> method.
		/// </param>
		void Unregister(SdkEventConsumer consumer);

	}

	[System.Serializable]
	public class SdkEventService : ISdkEventService
	{
		private readonly IConnectivityService _connectivityService;
		private List<SdkEvent> _events = new List<SdkEvent>();
		private List<SdkEvent> _phaseSpawnedEvents = new List<SdkEvent>();

		private Dictionary<string, List<SdkEventConsumer>> _sourceToConsumers =
		   new Dictionary<string, List<SdkEventConsumer>>();

		private Dictionary<SdkEvent, Promise> _eventToCompletion =
		   new Dictionary<SdkEvent, Promise>();

		private bool _isProcessing;

		public SdkEventService(IConnectivityService connectivityService)
		{
			_connectivityService = connectivityService;
		}

		public Promise Add(SdkEvent evt)
		{
			_eventToCompletion[evt] = new Promise();
			if (!_isProcessing)
			{
				_events.Add(evt);
				Process();
			}
			else
			{
				_phaseSpawnedEvents.Add(evt);
			}

			return _eventToCompletion[evt];
		}
		public void Process()
		{
			if (_isProcessing) return;
			_isProcessing = true;

			foreach (var evt in _events)
			{
				if (_sourceToConsumers.TryGetValue(evt.Source, out var consumers))
				{
					for (var i = consumers.Count - 1; i >= 0; i--)
					{
						try
						{
							var promise = consumers[i].Handler?.Invoke(evt);
							promise?.Merge(_eventToCompletion[evt]);
						}
						catch (Exception ex)
						{
							Debug.LogError("Failed to execute a Beamable SDK event");
							Debug.LogException(ex);
						}
					}
				}
			}

			_isProcessing = false;
			_events.Clear();

			if (_phaseSpawnedEvents.Count > 0)
			{
				_events.AddRange(_phaseSpawnedEvents);
				_phaseSpawnedEvents.Clear();
				Process();
			}
		}

		public SdkEventConsumer Register(string source, SdkEventHandler handler)
		{
			var consumer = new SdkEventConsumer
			{
				Source = source,
				Handler = handler,
				Service = this,
				RunLater = RunLater
			};
			if (!_sourceToConsumers.TryGetValue(source, out var consumers))
			{
				consumers = new List<SdkEventConsumer>();
			}

			consumers.Add(consumer);
			_sourceToConsumers[source] = consumers;
			return consumer;
		}

		public void Unregister(SdkEventConsumer consumer)
		{
			if (_sourceToConsumers.TryGetValue(consumer.Source, out var consumers))
			{
				consumers.Remove(consumer);
			}
		}

		private Promise RunLater(SdkEvent evt)
		{
			_connectivityService.OnReconnectOnce(() =>
			{
				Add(evt);
			});
			return Promise.Success;
		}
	}

	/// <summary>
	/// A function that consumes a <see cref="SdkEvent"/> and produces a <see cref="Promise"/>
	/// </summary>
	public delegate Promise SdkEventHandler(SdkEvent evt);

	public class SdkEventConsumer
	{
		/// <summary>
		/// The source of the consumer. This consumer only operates on <see cref="SdkEvent"/>'s that have the same <see cref="SdkEvent.Source"/> property.
		/// </summary>
		public string Source { get; set; }

		/// <summary>
		/// The <see cref="SdkEventHandler"/> that executes anytime a source matching <see cref="SdkEvent"/> is processed.
		/// </summary>
		public SdkEventHandler Handler { get; set; }

		/// <summary>
		/// A reference back to the <see cref="SdkEventService"/> that spawned this <see cref="SdkEventConsumer"/>.
		/// </summary>
		public SdkEventService Service { get; set; }

		internal SdkEventHandler RunLater
		{
			get;
			set;
		}

		/// <summary>
		/// A utility method to unregister the consumer from <see cref="SdkEventService"/>.
		/// </summary>
		public void Unsubscribe()
		{
			Service.Unregister(this);
		}

		/// <summary>
		/// A utility method that will add the <see cref="SdkEvent"/> back to the <see cref="SdkEventService"/> after internet connectivity has been established.
		/// </summary>
		/// <param name="evt">A <see cref="SdkEvent"/></param>
		/// <returns>A <see cref="Promise"/> that will complete when the <see cref="evt"/> has finished processing all of the <see cref="SdkEventConsumer"/>s</returns>
		public Promise RunAfterReconnection(SdkEvent evt)
		{
			return RunLater(evt);
		}
	}

	[Serializable]
	public class SdkEvent
	{
		[SerializeField]
		private string _source; // TODO: Can this be an enum?

		[SerializeField]
		private string _event; // TODO: Can this be an enum?

		[SerializeField]
		private string[] _args;

		/// <summary>
		/// The source of the event. This can be any string.
		/// An example of a Source value might be, "AuthService", or "InventoryService".
		/// </summary>
		public string Source => _source;

		/// <summary>
		/// The event name. This can be any string.
		/// A few examples of event names could be "register", "add", "remove", or "update".
		/// </summary>
		public string Event => _event;

		/// <summary>
		/// Events can have string parameters that represent context about the event.
		/// If the <see cref="SdkEvent"/> was representing a currency update, it might have 2 args, the currency id, and the amount to update.
		/// The arguments are kept in a simple string array so that they are always serializable.
		/// </summary>
		public string[] Args => _args;

		public SdkEvent()
		{

		}

		public SdkEvent(string source, string evt, params string[] args)
		{
			_source = source;
			_event = evt;
			_args = args;
		}
	}
}
