using Beamable.Common.Content;
using Beamable.Common.Inventory;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Beamable.Editor.Tests.Beamable.Content.ContentQueryTests
{
	public class AcceptTypeTests
	{
		[Test]
		public void DisallowsUnrelatedType()
		{
			var query = new ContentQuery
			{
				TypeConstraints = new HashSet<Type> { typeof(int) }
			};

			Assert.IsFalse(query.AcceptType<ItemContent>(false));
		}

		[Test]
		public void DisallowInheritedType()
		{
			var query = new ContentQuery
			{
				TypeConstraints = new HashSet<Type> { typeof(SubContent) }
			};

			Assert.IsFalse(query.AcceptType<ParentContent>(false));
		}

		[Test]
		public void AllowInheritedType()
		{
			var query = new ContentQuery
			{
				TypeConstraints = new HashSet<Type> { typeof(ParentContent) }
			};

			Assert.IsTrue(query.AcceptType<SubContent>(true));
		}

		[Test]
		public void AllowExact()
		{
			var query = new ContentQuery
			{
				TypeConstraints = new HashSet<Type> { typeof(SubContent) }
			};

			Assert.IsTrue(query.AcceptType<SubContent>(true));
		}

		[Test]
		public void AllowExact2()
		{
			var query = new ContentQuery
			{
				TypeConstraints = new HashSet<Type> { typeof(SubContent) }
			};

			Assert.IsTrue(query.AcceptType<SubContent>(false));
		}


		class ParentContent : ContentObject
		{

		}

		class SubContent : ParentContent
		{

		}
	}
}
