using Beamable.Common;
using Beamable.Common.Dependencies;
using Beamable.Coroutines;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Beamable.Tests.Runtime
{
	public class MockGameObjectContext : IGameObjectContext
	{
		public GameObject GameObject { get; }

		public MockGameObjectContext()
		{
			GameObject = new GameObject("mock-gob");
		}
	}

	public class DebouncerTests
	{
		private Debouncer _debouncer;
		private CoroutineService _coroutine;
		private int workCount;

		[SetUp]
		public void Setup()
		{
			workCount = 0;
			var builder = new DependencyBuilder();
			builder.AddSingleton<IGameObjectContext, MockGameObjectContext>();
			builder.AddComponentSingleton<CoroutineService>();
			builder.AddSingleton<Debouncer>();

			var provider = builder.Build();
			_debouncer = provider.GetService<Debouncer>();
			_coroutine = provider.GetService<CoroutineService>();

		}
		[UnityTest]
		public IEnumerator DebounceTest1()
		{
			var x = 0;
			var y = 0;
			for (var i = 0; i < 10; i++)
			{
				_debouncer.SetTimeout(() => { x++; }).Then(_ =>
				 {
					 Assert.AreEqual(1, x);
					 y++;
				 });
			}
			yield return new WaitForSecondsRealtime(.5f);
			Assert.AreEqual(1, x);
			Assert.AreEqual(10, y);

			_debouncer.SetTimeout(() => { x++; }).Then(_ => y++);
			yield return new WaitForSecondsRealtime(.5f);

			Assert.AreEqual(2, x);
			Assert.AreEqual(11, y);

		}

		[UnityTest]
		public IEnumerator DebounceTest2()
		{
			var set = new HashSet<int>();
			var x = 0;
			var callCount = 0;
			for (var i = 0; i < 10; i++)
			{
				set.Add(i);
				_debouncer.SetTimeout(() =>
				{
					callCount++;
					if (x == 0)
						x = set.Count;
				});
			}
			yield return new WaitForSecondsRealtime(.5f);

			Assert.AreEqual(1, callCount);
			Assert.AreEqual(10, x);
		}

		[UnityTest]
		public IEnumerator DebounceTest_AsyncStuff()
		{
			for (var i = 0; i < 10; i++)
			{
				var p = _debouncer.SetTimeout(Work);
			}

			yield return new WaitForSecondsRealtime(1);

			Promise Work()
			{
				workCount++;
				var p = new Promise().WaitForSeconds(.2f, _coroutine).ToPromise();
				return p;
			}

			Assert.AreEqual(1, workCount);
		}

	}
}
