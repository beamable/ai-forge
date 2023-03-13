using Beamable.Common;
using Beamable.Server.Editor.DockerCommands;
using System;
using System.Collections.Generic;

namespace Beamable.Server.Editor
{
	public enum MicroserviceState
	{
		IDLE, BUILDING, RUNNING, STOPPING
	}

	public enum MicroserviceCommand
	{
		BUILD, START, STOP, COMPLETE
	}

	class MicroserviceTransition
	{
		readonly MicroserviceState CurrentState;
		readonly MicroserviceCommand Command;

		public MicroserviceTransition(MicroserviceState currentState, MicroserviceCommand command)
		{
			CurrentState = currentState;
			Command = command;
		}

		public override int GetHashCode()
		{
			return 17 + 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var other = obj as MicroserviceTransition;
			return other != null && this.CurrentState == other.CurrentState && this.Command == other.Command;
		}
	}
}
