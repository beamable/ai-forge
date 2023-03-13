using Beamable.Api;
using Beamable.Common.Api;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Experimental.Api.Sim
{
	/// <summary>
	/// The <see cref="ISimFaultHandler"/>'s job to decide if a sequence of errors
	/// reported via the <see cref="HandleSyncError"/> function should qualify as an exception.
	/// </summary>
	public interface ISimFaultHandler
	{
		/// <summary>
		/// Report a sim failure to the fault handler.
		/// The method should maintain a set of failures since the last successful call to <see cref="HandleSyncSuccess"/>.
		///
		/// If this call results in an unrecoverable error, then the result's <see cref="SimFaultResult.Tolerable"/> field will be false.
		/// Otherwise, a true value means the error is recoverable.
		/// </summary>
		/// <param name="exception">The sim exception</param>
		/// <param name="newErrorReport">true if this results in a new error report. This will be false after
		/// the first call to <see cref="HandleSyncError"/> after each call to <see cref="HandleSyncSuccess"/></param>
		/// <returns>A <see cref="SimFaultResult"/> instance which has the <see cref="SimFaultResult.Tolerable"/> field and contains a <see cref="SimErrorReport"/> describing the set of errors so far.</returns>
		SimFaultResult HandleSyncError(Exception exception, out bool newErrorReport);

		/// <summary>
		/// Report that the sim state is healthy again.
		/// </summary>
		/// <returns>The last <see cref="SimErrorReport"/> that was being accumulated, or null if no errors had been reported. </returns>
		SimErrorReport HandleSyncSuccess();
	}

	public class DefaultSimFaultHandler : ISimFaultHandler
	{
		/// <summary>
		/// How long may the sim client be in a faulty state?
		/// </summary>
		public float MaxFaultyDurationInSeconds { get; set; } = 15;
		protected SimErrorReport report;
		protected bool HasError => report != null;

		protected HashSet<long> allowedPlatformErrorCodes = new HashSet<long>(new long[] { 502, 504 });

		public virtual SimErrorReport HandleSyncSuccess()
		{
			var oldReport = report;
			report = null;
			return oldReport;
		}

		public virtual SimFaultResult HandleSyncError(Exception exception, out bool newErrorReport)
		{
			newErrorReport = !HasError;
			AddErrorInstance(exception);

			switch (exception)
			{
				case NoConnectivityException _:
				case RequesterException platformError when allowedPlatformErrorCodes.Contains(platformError.Status):
					// allow these errors...
					break;
				default:
					// the usual behaviour is for the exception to be thrown, so lets throw this if we can't handle it.
					return new SimFaultResult
					{
						Tolerable = false,
						ErrorMessage = "Encountered a fatal server error",
						Report = GetErrorReport()
					};
			}

			var now = GetRealtimeSinceStartup();
			var duration = now - GetErrorReport().createdAtSeconds;
			if (duration > MaxFaultyDurationInSeconds)
			{
				// we have waited too long- allow the error to actually fail the networking.
				return new SimFaultResult
				{
					Tolerable = false,
					ErrorMessage = "Failed to reconnect to servers.",
					Report = GetErrorReport()
				};
			}

			return new SimFaultResult
			{
				Tolerable = true,
				ErrorMessage = "Reconnecting to servers...",
				Report = GetErrorReport()
			};
		}

		protected SimErrorReport GetErrorReport()
		{
			if (report == null)
			{
				report = new SimErrorReport
				{
					createdAtSeconds = GetRealtimeSinceStartup()
				};
			}
			return report;
		}

		protected void AddErrorInstance(Exception ex)
		{
			GetErrorReport().errors.Add(new SimErrorInstance
			{
				reportedAtSeconds = GetRealtimeSinceStartup(),
				exception = ex
			});
		}

		protected float GetRealtimeSinceStartup() => Time.realtimeSinceStartup;

	}

	public class SimErrorReport
	{
		/// <summary>
		/// the realtime in seconds when the error report was created
		/// </summary>
		public float createdAtSeconds;

		/// <summary>
		/// A set of <see cref="SimErrorInstance"/> instances describing the various errors
		/// </summary>
		public List<SimErrorInstance> errors = new List<SimErrorInstance>();
	}

	public class SimErrorInstance
	{
		/// <summary>
		/// the realtime in seconds that the error was received
		/// </summary>
		public float reportedAtSeconds;

		/// <summary>
		/// the sim exception
		/// </summary>
		public Exception exception;
	}

	public class SimNetworkErrorException : Exception
	{
		public SimErrorReport Report { get; }
		public SimNetworkErrorException(SimErrorReport report) : base("The simClient has failed to recover from an error. Check report for details.")
		{
			Report = report;
		}
	}

	public struct SimFaultResult
	{
		/// <summary>
		/// True if the fault was handled correctly, false if the error should cause an outage in the service.
		/// </summary>
		public bool Tolerable;

		/// <summary>
		/// The error message that should be displayed in the event that <see cref="Tolerable"/> is false.
		/// </summary>
		public string ErrorMessage;

		/// <summary>
		/// A <see cref="SimErrorReport"/> instance containing all of the failures that occured.
		/// </summary>
		public SimErrorReport Report;
	}
}
