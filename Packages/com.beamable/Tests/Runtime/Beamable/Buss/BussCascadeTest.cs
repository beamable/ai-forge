using Beamable.UI.Buss;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beamable.Tests.Buss
{
	public class BussCascadeTest
	{
		private GameObject _root;
		private BussStyleSheet styleSheetA, styleSheetB;
		private Dictionary<string, BussElement> _bussElements = new Dictionary<string, BussElement>();

		/*
		 * Hierarchy:
		 *	A
		 *		A_A
		 *		A_B (class1)
		 *	B
		 *		B_B (class2)
		 *			B_B_B (class2, class3)
		*/
		[SetUp]
		public void Init()
		{
			InitStyleSheets();
			InitHierarchy();
		}

		private void InitStyleSheets()
		{
			styleSheetA = ScriptableObject.CreateInstance<BussStyleSheet>();
			styleSheetA.Styles.Add(BussStyleRule.Create("#A", new BussPropertyProvider[]
			{
				BussPropertyProvider.Create(BussStyle.Threshold.Key, new FloatBussProperty(10f)),
			}.ToList()));
			styleSheetA.Styles.Add(BussStyleRule.Create("#A > *", new BussPropertyProvider[]
			{
				BussPropertyProvider.Create(BussStyle.Threshold.Key, new FloatBussProperty(15f)),
			}.ToList()));
			styleSheetA.Styles.Add(BussStyleRule.Create("#A > .class1", new BussPropertyProvider[]
			{
				BussPropertyProvider.Create(BussStyle.Threshold.Key, new FloatBussProperty(20f)),
			}.ToList()));

			styleSheetB = ScriptableObject.CreateInstance<BussStyleSheet>();
			styleSheetB.Styles.Add(BussStyleRule.Create("*", new BussPropertyProvider[]
			{
				BussPropertyProvider.Create(BussStyle.BorderWidth.Key, new FloatBussProperty(44f)),
			}.ToList()));
			styleSheetB.Styles.Add(BussStyleRule.Create("#B", new BussPropertyProvider[]
			{
				BussPropertyProvider.Create(BussStyle.Threshold.Key, new FloatBussProperty(15f)),
			}.ToList()));
			styleSheetB.Styles.Add(BussStyleRule.Create("#B *", new BussPropertyProvider[]
			{
				BussPropertyProvider.Create(BussStyle.Threshold.Key, new FloatBussProperty(20f)),
			}.ToList()));
			styleSheetB.Styles.Add(BussStyleRule.Create("#B, #B_B_B", new BussPropertyProvider[]
			{
				BussPropertyProvider.Create(BussStyle.BorderWidth.Key, new FloatBussProperty(66f)),
			}.ToList()));
		}

		private void InitHierarchy()
		{
			_root = new GameObject("Buss Cascade Test Root");
			var a = CreateBussElement(_root, styleSheetA, "A");
			CreateBussElement(a, null, "A_A");
			CreateBussElement(a, null, "A_B", "class1");
			var b = CreateBussElement(_root, styleSheetB, "B");
			var b_b = CreateBussElement(b, null, "B_B", "class2");
			CreateBussElement(b_b, null, "B_B_B", "class3", "class2");
		}

		private GameObject CreateBussElement(GameObject parent, BussStyleSheet styleSheet, string id, params string[] classes)
		{
			var go = new GameObject(id, typeof(BussElement));
			go.transform.parent = parent.transform;
			var bussElement = go.GetComponent<BussElement>();
			bussElement.Id = id;
			foreach (string @class in classes)
			{
				bussElement.AddClass(@class);
			}
			bussElement.StyleSheet = styleSheet;
			_bussElements[id] = bussElement;
			return go;
		}

		[TearDown]
		public void Cleanup()
		{
			Object.Destroy(_root);
			Object.Destroy(styleSheetA);
			Object.Destroy(styleSheetB);
		}

		[Test(Description = "Selector '#A' should set threshold to 10.")]
		public void IdSelector()
		{
			Assert.AreEqual(10f, BussStyle.Threshold.Get(_bussElements["A"].Style).FloatValue);
		}

		[Test(Description = "Selector '#A > *' should set threshold to 15.")]
		public void ParentedSelector()
		{
			Assert.AreEqual(15f, BussStyle.Threshold.Get(_bussElements["A_A"].Style).FloatValue);
		}

		[Test(Description = "Selector '#A > .class1' should set threshold to 20, overriding selector '#A > *' with value 15.")]
		public void SelectorWeight()
		{
			Assert.AreEqual(20f, BussStyle.Threshold.Get(_bussElements["A_B"].Style).FloatValue);
		}

		[Test(Description = "Selector '#B *' should set threshold to 20.")]
		public void ParentedNonDirectSelector()
		{
			Assert.AreEqual(15f, BussStyle.Threshold.Get(_bussElements["B"].Style).FloatValue);
			Assert.AreEqual(20f, BussStyle.Threshold.Get(_bussElements["B_B"].Style).FloatValue);
			Assert.AreEqual(20f, BussStyle.Threshold.Get(_bussElements["B_B_B"].Style).FloatValue);
		}

		[Test(Description = "Selector '#B, #B_B_B' should set border width to 66.")]
		public void AlternativeSelector()
		{
			Assert.AreEqual(66f, BussStyle.BorderWidth.Get(_bussElements["B"].Style).FloatValue);
			Assert.AreEqual(44f, BussStyle.BorderWidth.Get(_bussElements["B_B"].Style).FloatValue);
			Assert.AreEqual(66f, BussStyle.BorderWidth.Get(_bussElements["B_B_B"].Style).FloatValue);
		}
	}
}
