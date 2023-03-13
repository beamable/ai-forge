using Beamable.Common.Pooling;
using NUnit.Framework;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace Beamable.Tests.Runtime.Pooling.StringBuilderPoolTests
{

	public class SpawnTests
	{
		[UnityTest]
		public IEnumerator MultithreadedAccess()
		{

			// spawn a few threads, and have them battle over the StringBuilderPool.Instance.Spawn function, and make sure nothing blows up.

			var threadCount = 3;
			const int cyclesPerThread = 2500;
			void Launch()
			{
				var c = cyclesPerThread;
				var t = new Thread(() =>
				{
					try
					{
						while (c-- > 0)
						{
							using (var _ = StringBuilderPool.StaticPool.Spawn())
							{
								// do nothing...
								Thread.Sleep(1);
							}
						}
					}
					catch (Exception ex)
					{
						Assert.Fail("Exception thrown. " + ex.Message + " " + ex.StackTrace);

					}
				});
				t.Start();
			}

			for (var i = 0; i < threadCount; i++)
			{
				Launch();
			}

			yield return new WaitForSecondsRealtime(3);
		}
	}
}
