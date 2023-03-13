using Beamable.Serialization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.PerformanceTesting;
using UnityEngine;

namespace Beamable.Editor.Tests
{
	public class JsonPerfTesting
	{
		[Performance]
		[Test]
		public void UnityJson()
		{
			var count = 1000;
			var person = new Person
			{
				age = 101,
				first = "Winston",
				last = "Wax",
				hobbies = new List<Hobby>() { new Hobby { name = "Saxaphone" } }
			};

			Measure.Method(() =>
				   {
					   for (var i = 0; i < count; i++)
					   {
						   var json = JsonUtility.ToJson(person);
						   // var mirror = JsonUtility.FromJson<Person>(json);
					   }
				   })
				   .GC()
				   .MeasurementCount(10)
				   .Run();
		}

		[Performance]
		[Test]
		public void BeamableJson()
		{
			var count = 1000;
			var person = new Person
			{
				age = 101,
				first = "Winston",
				last = "Wax",
				hobbies = new List<Hobby>() { new Hobby { name = "Saxaphone" } }
			};

			JsonSerializable.JsonSaveStream.Preallocate(10);
			Measure.Method(() =>
				   {
					   for (var i = 0; i < count; i++)
					   {
						   var json = JsonSerializable.ToJson(person);
						   // var mirror = JsonSerializable.FromJson<Person>(json);
					   }
				   })
				   .GC()
				   .MeasurementCount(10)
				   .Run();
		}

		[Performance]
		[Test]
		public void StringBuilder()
		{
			var sb = new StringBuilder(64);
			Measure.Method(() =>
				   {

					   for (var i = 0; i < 1000; i++)
					   {
						   sb.Append("hello world");
					   }

					   var str = sb.ToString();
				   })
				   .GC()
				   .MeasurementCount(10)
				   .Run();
		}


		[Serializable]
		public class Person : JsonSerializable.ISerializable
		{
			public int age;
			public string first;
			public string last;
			public List<Hobby> hobbies;

			public void Serialize(JsonSerializable.IStreamSerializer s)
			{
				s.Serialize("age", ref age);
				s.Serialize("first", ref first);
				s.Serialize("last", ref last);
				s.SerializeKnownList("hobbies", ref hobbies);

			}

		}

		[Serializable]
		public class Hobby : JsonSerializable.ISerializable
		{
			public string name;

			public void Serialize(JsonSerializable.IStreamSerializer s)
			{
				s.Serialize("name", ref name);
			}
		}
	}
}
