using Beamable.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beamable.Platform.Tests
{
	public static class TestUtil
	{
		public static WaitForPromise AsYield(this PromiseBase arg, float timeout = .2f)
		{
			return new WaitForPromise(arg, timeout);
		}

		public static string GetField(this WWWForm form, string field)
		{
			var fieldNames = typeof(WWWForm).GetField("fieldNames", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form) as List<string>;
			var formData = typeof(WWWForm).GetField("formData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form) as List<byte[]>;

			var formIndex = fieldNames.IndexOf(field);
			if (formIndex == -1)
			{
				throw new Exception($"Form does not contain value for {field}");
			}
			var data = formData[formIndex];
			var decoded = Encoding.UTF8.GetString(data);
			return decoded;
		}
	}

	public class WaitForPromise : CustomYieldInstruction
	{
		private readonly PromiseBase _promise;
		private float _murderAt;

		public WaitForPromise(PromiseBase promise, float timeOut = .2f)
		{
			_promise = promise;

			_murderAt = Time.realtimeSinceStartup + timeOut;
		}

		public override bool keepWaiting
		{
			get
			{
				if (Time.realtimeSinceStartup > _murderAt)
				{
					Debug.LogError("Yielded timeout");
					return false;
				}
				return !_promise.IsCompleted;
			}
		}
	}
}
