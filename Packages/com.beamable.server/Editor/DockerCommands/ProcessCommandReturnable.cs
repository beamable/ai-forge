using Beamable.Common;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Beamable.Server.Editor.DockerCommands
{
	public abstract class DockerCommandReturnable<T> : DockerCommand
	{

		protected Promise<T> Promise { get; private set; }


		protected bool _finished;

		public override void Start()
		{
			StartAsync();
		}

		public virtual Promise<T> StartAsync()
		{
			if (!_skipDockerCheck && DockerRequired && DockerNotInstalled)
			{
				return Promise<T>.Failed(new DockerNotInstalledException());
			}

			Promise = new Promise<T>();
			base.Start();

			return Promise;
		}

		protected abstract void Resolve();

		protected override void HandleOnExit()
		{
			void Callback()
			{
				base.HandleOnExit();
				Resolve();
			}

			BeamEditorContext.Default.Dispatcher.Schedule(Callback);
			_finished = true;
		}
	}

}
