using Beamable.Common.Dependencies;
using NUnit.Framework;

namespace Beamable.Tests.Runtime.Dependencies
{
	public class ScopingTests
	{

		[SetUp]
		public void Setup()
		{
			A.instanceCount = 0;
			B.instanceCount = 0;
			C.instanceCount = 0;
		}

		[Test]
		public void ASingletonInAParentExistsOnce()
		{
			var builder = new DependencyBuilder();
			builder.AddSingleton<A>();
			var provider = builder.Build();
			var sub = provider.Fork();

			// nothing exists yet
			Assert.AreEqual(0, A.instanceCount);

			// the two scopes provide the identical instance
			Assert.AreEqual(provider.GetService<A>(), sub.GetService<A>());
			Assert.AreEqual(1, A.instanceCount);

			// and asking for it against doesn't recreate it
			Assert.AreEqual(provider.GetService<A>(), sub.GetService<A>());
			Assert.AreEqual(1, A.instanceCount);

		}

		[Test]
		public void AScopedInAParentExistsTwice()
		{
			var builder = new DependencyBuilder();
			builder.AddScoped<A>();
			var provider = builder.Build();
			var sub = provider.Fork();

			// nothing exists yet
			Assert.AreEqual(0, A.instanceCount);

			// the two scopes provide different services
			Assert.AreNotEqual(provider.GetService<A>(), sub.GetService<A>());
			Assert.AreEqual(2, A.instanceCount);

			// the services are not re-created after asking for them again and again
			Assert.AreNotEqual(provider.GetService<A>(), sub.GetService<A>());
			Assert.AreEqual(2, A.instanceCount);

		}


		[Test]
		public void ATransientInAParentExistsSoManyTimes()
		{
			var builder = new DependencyBuilder();
			builder.AddTransient<A>();
			var provider = builder.Build();
			var sub = provider.Fork();

			// nothing exists yet
			Assert.AreEqual(0, A.instanceCount);

			// the parent scope even provides different services
			Assert.AreNotEqual(provider.GetService<A>(), provider.GetService<A>());
			Assert.AreEqual(2, A.instanceCount);

			// the two scopes provide different services
			Assert.AreNotEqual(provider.GetService<A>(), sub.GetService<A>());
			Assert.AreEqual(4, A.instanceCount);


			// the services are recreated every time they're asked for
			Assert.AreNotEqual(provider.GetService<A>(), sub.GetService<A>());
			Assert.AreEqual(6, A.instanceCount);
		}

		[Test]
		public void Dependencies_TransientDependsOnSingleton()
		{
			var builder = new DependencyBuilder();
			builder.AddSingleton<A>();
			builder.AddTransient<B>();
			var provider = builder.Build();

			// nothing exists yet
			Assert.AreEqual(0, A.instanceCount);
			Assert.AreEqual(0, B.instanceCount);

			// creating multiple B's doesn't create multiple A's
			Assert.AreNotEqual(provider.GetService<B>(), provider.GetService<B>());
			Assert.AreEqual(1, A.instanceCount);
			Assert.AreEqual(2, B.instanceCount);

			// the b services use the same A instance
			Assert.AreEqual(provider.GetService<B>().A, provider.GetService<B>().A);
			Assert.AreEqual(4, B.instanceCount);
			Assert.AreEqual(1, A.instanceCount);

		}

		[Test]
		public void Dependencies_SingletonDependsOnTransient()
		{
			var builder = new DependencyBuilder();
			builder.AddTransient<A>();
			builder.AddSingleton<B>();
			var provider = builder.Build();


			// nothing exists yet
			Assert.AreEqual(0, A.instanceCount);
			Assert.AreEqual(0, B.instanceCount);

			// there is only one B, and its the same
			Assert.AreEqual(provider.GetService<B>(), provider.GetService<B>());
			Assert.AreEqual(1, A.instanceCount);
			Assert.AreEqual(1, B.instanceCount);

			// the b services use the same transient instance
			Assert.AreEqual(provider.GetService<B>().A, provider.GetService<B>().A);
			Assert.AreEqual(1, B.instanceCount);

			// asking for more A's gives unique ones
			var a = provider.GetService<A>();
			Assert.AreEqual(2, A.instanceCount);
			Assert.AreNotEqual(provider.GetService<B>().A, a);
		}

		[Test]
		public void ScopedDependencies_ScopedDependsOnSingleton()
		{
			var builder = new DependencyBuilder();
			builder.AddSingleton<A>();
			builder.AddScoped<B>();
			var provider = builder.Build();
			var sub = provider.Fork();

			// nothing exists yet
			Assert.AreEqual(0, A.instanceCount);
			Assert.AreEqual(0, B.instanceCount);

			// the two scopes have different B's,
			Assert.AreNotEqual(provider.GetService<B>(), sub.GetService<B>());
			Assert.AreEqual(1, A.instanceCount);
			Assert.AreEqual(2, B.instanceCount);

			// but share the common A singleton
			Assert.AreEqual(provider.GetService<B>().A, sub.GetService<B>().A);
			Assert.AreEqual(2, B.instanceCount);
			Assert.AreEqual(1, A.instanceCount);
		}


		[Test]
		public void ScopedDependencies_SingletonDependsOnScoped()
		{
			var builder = new DependencyBuilder();
			builder.AddScoped<A>();
			builder.AddSingleton<B>();
			var provider = builder.Build();
			var sub = provider.Fork();

			// nothing exists yet
			Assert.AreEqual(0, A.instanceCount);
			Assert.AreEqual(0, B.instanceCount);

			// the two services have the same A, because the singleton has already been sealed
			Assert.AreEqual(provider.GetService<B>().A, sub.GetService<B>().A);
			Assert.AreEqual(1, B.instanceCount);
			Assert.AreEqual(1, A.instanceCount);

			// the two scopes have the same B,
			Assert.AreEqual(provider.GetService<B>(), sub.GetService<B>());
			Assert.AreEqual(1, A.instanceCount);
			Assert.AreEqual(1, B.instanceCount);

			// fork a new one, but over-write the singleton instance
			var sub2 = provider.Fork(b =>
										 b.RemoveIfExists<B>()
										  .AddSingleton<B>());

			Assert.AreNotEqual(provider.GetService<B>().A, sub2.GetService<B>().A);
			Assert.AreEqual(2, B.instanceCount);
			Assert.AreEqual(2, A.instanceCount);

		}

		public class Counter
		{
			private int count;

		}

		public class A : Counter
		{
			public static int instanceCount = 0;

			public A()
			{
				instanceCount++;
			}

		}
		public class B : Counter
		{
			public A A { get; }
			public static int instanceCount = 0;

			public B(A a)
			{
				A = a;
				instanceCount++;
			}

		}
		public class C : Counter
		{
			public static int instanceCount = 0;

			public C()
			{
				instanceCount++;
			}

		}
	}
}
