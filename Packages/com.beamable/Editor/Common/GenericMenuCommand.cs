using System;

namespace Beamable.Editor.Common
{
	public class GenericMenuCommand
	{
		private readonly Action _onClick;

		public string Name { get; }

		public GenericMenuCommand(string name, Action onClick)
		{
			Name = name;
			_onClick = onClick;
		}

		public void Invoke()
		{
			_onClick?.Invoke();
		}
	}
}
