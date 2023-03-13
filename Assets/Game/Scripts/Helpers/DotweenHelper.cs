using Beamable.Common;
using DG.Tweening;

namespace LDAP.Utils
{
	public static class DotweenHelper
	{
		public static Promise ToPromise(this Tweener tween)
		{
			var promise = new Promise();
			tween.OnKill(() =>
			{
				promise.CompleteSuccess();
			});
			return promise;
		}
	}
}