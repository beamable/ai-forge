using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Beamable.Api
{
	/**
     * Creates an Error code of the form Code:SystemID:ModuleID:LineNumber. This should let us expose a number to
     * players in a fairly obfuscated way while still letting those numbers be useful to us to diagnose where a
     * particular error could be coming from.
     */
	public class ErrorCode : System.Exception
	{
		public long Code { get; }
		public GameSystem System { get; }
		public string Module { get; }
		public int LineNumber { get; }
		public string Error { get; }
		public override string Message { get; }


		int SystemId
		{
			get { return (int)System; }
		}

		public ErrorCode(long code, GameSystem system = GameSystem.GAME_CLIENT, string error = "", string message = "")
		{
			Code = code;
			System = system;

			var callStack = new StackFrame(1, true);
			Module = Path.GetFileNameWithoutExtension(callStack.GetFileName());
			Module = string.IsNullOrEmpty(Module) ? "-1" : Regex.Replace(Module, "[^A-Z]", "");
			LineNumber = callStack.GetFileLineNumber();
			Error = error;
			Message = message;
		}

		public ErrorCode(PlatformError err)
		{
			Code = err.status;
			System = GameSystem.PLATFORM;
			Module = err.service;
			LineNumber = 0;
			Error = err.error;
			Message = err.message;
		}

		public override string ToString()
		{
			return string.Format("{0}:{1}:{2}:{3}:{4}", Code, SystemId, Module, LineNumber, Error);
		}
	}

	public enum GameSystem
	{
		GAME_CLIENT = 1,
		GAME_SERVER = 2,
		PLATFORM = 3
	}
}
