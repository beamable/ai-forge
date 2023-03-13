using Beamable.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Beamable.Editor.Tests.Common
{
	public class TryInvokeCallbackTests
	{
		[SetUp]
		public void Setup()
		{
			BaseClass.InvokeTunaCount = 0;
			BaseClass.InvokeFishCount = 0;
		}

		[Test]
		public void CanFindPublicMethod()
		{
			var instance = new BaseClass();
			Assert.IsTrue(instance.TryInvokeCallback(nameof(BaseClass.Tuna)));
			Assert.AreEqual(BaseClass.InvokeTunaCount, 1);
		}

		[Test]
		public void CanFindPrivateMethod()
		{
			var instance = new BaseClass();
			Assert.IsTrue(instance.TryInvokeCallback("Fish"));
			Assert.AreEqual(BaseClass.InvokeFishCount, 1);
		}

		[Test]
		public void CanFindPublicMethodFromBase()
		{
			var instance = new SubClass();
			Assert.IsTrue(instance.TryInvokeCallback(nameof(BaseClass.Tuna)));
			Assert.AreEqual(BaseClass.InvokeTunaCount, 1);
		}

		[Test]
		public void CanFindPrivateMethodFromBase()
		{
			var instance = new SubClass();
			Assert.IsTrue(instance.TryInvokeCallback("Fish"));
			Assert.AreEqual(BaseClass.InvokeFishCount, 1);
		}

		[Test]
		public void ExpectAFailureWhenNoMethodExists()
		{
			var instance = new SubClass();
			LogAssert.Expect(LogType.Error, "Callback method not found");
			Assert.IsFalse(instance.TryInvokeCallback("DoesNotExist"));
			Assert.AreEqual(BaseClass.InvokeFishCount, 0);
			Assert.AreEqual(BaseClass.InvokeTunaCount, 0);

		}

		[Test]
		public void ExpectAFailureWhenMethodHasParams()
		{
			var instance = new SubClass();
			LogAssert.Expect(LogType.Error, "Callback method cannot not have parameters.");
			Assert.IsFalse(instance.TryInvokeCallback(nameof(SubClass.HasParams)));
			Assert.AreEqual(BaseClass.InvokeFishCount, 0);
		}
	}


	class BaseClass
	{
		public static long InvokeTunaCount = 0;
		public static long InvokeFishCount = 0;
		public void Tuna()
		{
			InvokeTunaCount++;
		}

		private void Fish()
		{
			InvokeFishCount++;
		}
	}

	class SubClass : BaseClass
	{
		public void HasParams(int a)
		{
			InvokeFishCount++;
		}
	}
}
