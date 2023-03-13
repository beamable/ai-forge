using UnityEngine;

namespace Beamable.Coroutines
{
	public class CompletedYieldInstruction : CustomYieldInstruction
	{
		public static readonly CompletedYieldInstruction Instance = new CompletedYieldInstruction();

		public override bool keepWaiting
		{
			get { return false; }
		}
	}
}
