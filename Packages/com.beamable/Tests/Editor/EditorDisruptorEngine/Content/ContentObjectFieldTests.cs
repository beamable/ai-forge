using Beamable.Common.Content;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Beamable.Editor.Tests.Beamable.Content
{
	public class ContentObjectFieldTests
	{
		/// <summary>
		/// Tests that all public fields of ContentObject subclasses
		/// have a tooltip
		///
		/// NOTE: THis does NOT test "nested types" (e.g. Loop through the fields OF THE FIELDS)
		/// 
		/// </summary>
		[Test]
		public void ContentObjects_EachPublicFieldHasTooltip()
		{
			var contentObjectSubclasses =
				typeof(ContentObject).Assembly.GetTypes()
				.Where(type => type.IsSubclassOf(typeof(ContentObject)));

			int invalidClassesCount = 0;
			foreach (Type type in contentObjectSubclasses)
			{
				//Keep commented-out unless a dev is debugging this test
				//Debug.Log($"{type.Name}\n");

				var fieldInfos = type.GetFields().ToList();

				int invalidFieldInfoCount = 0;
				foreach (FieldInfo fieldInfo in fieldInfos)
				{
					HideInInspector hideInInspector = fieldInfo.GetCustomAttribute<HideInInspector>();
					if (hideInInspector != null)
					{
						// Ignore [HideInInspector] fields
						continue;
					}

					TooltipAttribute tooltipAttribute = fieldInfo.GetCustomAttribute(typeof(TooltipAttribute)) as TooltipAttribute;
					if (tooltipAttribute == null || tooltipAttribute.tooltip.Length == 0)
					{
						Debug.LogWarning($"\t{type.Name} - MUST Add [Tooltip] on" +
									   $" {fieldInfo.Name}\n");

						invalidFieldInfoCount++;
					}
					else
					{
						//Keep commented-out unless a dev is debugging this test
						//Debug.Log($"\t{type.Name} - Has [Tooltip] on" +
						//              $" {fieldInfo.Name} of {tooltipAttribute.tooltip}\n");

					}
				}

				if (invalidFieldInfoCount > 0)
				{
					Debug.LogWarning($"NO {type.Name} - Is NOT Valid because " +
							  $"{invalidFieldInfoCount} of {fieldInfos.Count} incorrect.\n");
					invalidClassesCount++;
				}
				else
				{
					//Keep commented-out unless a dev is debugging this test
					//Debug.LogWarning($"YES - {type.Name} - Is Valid.\n");
				}
			}

			if (invalidClassesCount > 0)
			{
				Assert.That(invalidClassesCount, Is.EqualTo(0),
					$"Test failed. {invalidClassesCount} classes are NOT valid, per tooltips.\n");
			}
		}
	}
}
