#if !UNITY_WEBGL
using System;
using System.Threading.Tasks;

namespace Beamable.Common
{
	public class BeamableTaskExtensions
	{
		public static Task TaskFromPromise<T>(Promise<T> promise)
		{
			var tcs = new System.Threading.Tasks.TaskCompletionSource<T>();
			promise.Then(obj =>
			{
				tcs.SetResult(obj);

			}).Error((Exception e) =>
			{
				tcs.SetException(e);
			});

			return tcs.Task;
		}
	}
}
#endif
