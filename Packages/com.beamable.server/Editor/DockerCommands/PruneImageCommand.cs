
namespace Beamable.Server.Editor.DockerCommands
{
	public class PruneImageCommand : DockerCommandReturnable<bool>
	{
		private readonly IDescriptor _descriptor;
		private readonly bool _all;
		private readonly int _secondsOld;

		/// <summary>
		/// Creates a command that will clean old built images of the given descriptor off your computer
		/// </summary>
		/// <param name="descriptor">The descriptor to clean. Images built for this descriptor will be removed</param>
		/// <param name="all">if true, all found images matching the descriptor will be removed</param>
		/// <param name="secondsOld">the number of seconds old the image needs to be to be deleted</param>
		public PruneImageCommand(IDescriptor descriptor, bool all = true, int secondsOld = 120)
		{
			_descriptor = descriptor;
			_all = all;
			_secondsOld = secondsOld;
		}

		public override string GetCommandString()
		{
			var untilFilter = "";
			if (_secondsOld > 0)
			{
				untilFilter = $"--filter \"until={_secondsOld}s\"";
			}

			var cmd = $"{DockerCmd} image prune --filter \"label=beamable-service-name={_descriptor.Name}\" {untilFilter} -f {(_all ? "-a" : "")}";
			return cmd;
		}

		protected override void Resolve()
		{
			Promise.CompleteSuccess(true);
		}
	}
}
