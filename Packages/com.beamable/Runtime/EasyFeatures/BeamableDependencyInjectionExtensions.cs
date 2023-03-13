using Beamable.Common.Dependencies;

public static partial class BeamableDependencyInjectionExtensions
{
	public static void ReplaceAsSingleton<TInterface, TImplementation>(this IDependencyBuilder builder) where TImplementation : TInterface
	{
		builder.RemoveIfExists<TInterface>();
		builder.AddSingleton<TInterface, TImplementation>();
	}

	public static void ReplaceAsScoped<TInterface, TImplementation>(this IDependencyBuilder builder) where TImplementation : TInterface
	{
		builder.RemoveIfExists<TInterface>();
		builder.AddScoped<TInterface, TImplementation>();
	}

	public static void ReplaceAsTransient<TInterface, TImplementation>(this IDependencyBuilder builder) where TImplementation : TInterface
	{
		builder.RemoveIfExists<TInterface>();
		builder.AddTransient<TInterface, TImplementation>();
	}

	public static void RedirectAsSingleton<TInterface, TImplementation>(this IDependencyBuilder builder) where TImplementation : TInterface
	{
		builder.RemoveIfExists<TInterface>();
		builder.AddSingleton<TInterface, TImplementation>((provider) => provider.GetService<TImplementation>());
	}

	public static void RedirectAsScoped<TInterface, TImplementation>(this IDependencyBuilder builder) where TImplementation : TInterface
	{
		builder.RemoveIfExists<TInterface>();
		builder.AddScoped<TInterface, TImplementation>((provider) => provider.GetService<TImplementation>());
	}

	public static void RedirectAsTransient<TInterface, TImplementation>(this IDependencyBuilder builder) where TImplementation : TInterface
	{
		builder.RemoveIfExists<TInterface>();
		builder.AddTransient<TInterface, TImplementation>((provider) => provider.GetService<TImplementation>());
	}

	public static void SetupUnderlyingSystemSingleton<TUnderlyingSystem, TI1>(this IDependencyBuilder builder) where TUnderlyingSystem : TI1
	{
		builder.RemoveIfExists<TUnderlyingSystem>();
		builder.AddSingleton<TUnderlyingSystem>();
		builder.RedirectAsSingleton<TI1, TUnderlyingSystem>();
	}

	public static void SetupUnderlyingSystemScoped<TUnderlyingSystem, TI1>(this IDependencyBuilder builder) where TUnderlyingSystem : TI1
	{
		builder.RemoveIfExists<TUnderlyingSystem>();
		builder.AddScoped<TUnderlyingSystem>();
		builder.RedirectAsScoped<TI1, TUnderlyingSystem>();
	}

	public static void SetupUnderlyingSystemTransient<TUnderlyingSystem, TI1>(this IDependencyBuilder builder) where TUnderlyingSystem : TI1
	{
		builder.RemoveIfExists<TUnderlyingSystem>();
		builder.AddTransient<TUnderlyingSystem>();
		builder.RedirectAsTransient<TI1, TUnderlyingSystem>();
	}

	public static void SetupUnderlyingSystemSingleton<TUnderlyingSystem, TI1, TI2>(this IDependencyBuilder builder) where TUnderlyingSystem : TI1, TI2
	{
		builder.RemoveIfExists<TUnderlyingSystem>();
		builder.AddSingleton<TUnderlyingSystem>();

		builder.RedirectAsSingleton<TI1, TUnderlyingSystem>();
		builder.RedirectAsSingleton<TI2, TUnderlyingSystem>();
	}

	public static void SetupUnderlyingSystemScoped<TUnderlyingSystem, TI1, TI2>(this IDependencyBuilder builder) where TUnderlyingSystem : TI1, TI2
	{
		builder.RemoveIfExists<TUnderlyingSystem>();
		builder.AddScoped<TUnderlyingSystem>();

		builder.RedirectAsScoped<TI1, TUnderlyingSystem>();
		builder.RedirectAsScoped<TI2, TUnderlyingSystem>();
	}

	public static void SetupUnderlyingSystemTransient<TUnderlyingSystem, TI1, TI2>(this IDependencyBuilder builder) where TUnderlyingSystem : TI1, TI2
	{
		builder.RemoveIfExists<TUnderlyingSystem>();
		builder.AddTransient<TUnderlyingSystem>();

		builder.RedirectAsTransient<TI1, TUnderlyingSystem>();
		builder.RedirectAsTransient<TI2, TUnderlyingSystem>();
	}


	public static void SetupUnderlyingSystemSingleton<TUnderlyingSystem, TI1, TI2, TI3>(this IDependencyBuilder builder) where TUnderlyingSystem : TI1, TI2, TI3
	{
		builder.RemoveIfExists<TUnderlyingSystem>();
		builder.AddSingleton<TUnderlyingSystem>();

		builder.RedirectAsSingleton<TI1, TUnderlyingSystem>();
		builder.RedirectAsSingleton<TI2, TUnderlyingSystem>();
		builder.RedirectAsSingleton<TI3, TUnderlyingSystem>();
	}

	public static void SetupUnderlyingSystemScoped<TUnderlyingSystem, TI1, TI2, TI3>(this IDependencyBuilder builder) where TUnderlyingSystem : TI1, TI2, TI3
	{
		builder.RemoveIfExists<TUnderlyingSystem>();
		builder.AddScoped<TUnderlyingSystem>();

		builder.RedirectAsScoped<TI1, TUnderlyingSystem>();
		builder.RedirectAsScoped<TI2, TUnderlyingSystem>();
		builder.RedirectAsScoped<TI3, TUnderlyingSystem>();
	}

	public static void SetupUnderlyingSystemTransient<TUnderlyingSystem, TI1, TI2, TI3>(this IDependencyBuilder builder) where TUnderlyingSystem : TI1, TI2, TI3
	{
		builder.RemoveIfExists<TUnderlyingSystem>();
		builder.AddTransient<TUnderlyingSystem>();

		builder.RedirectAsTransient<TI1, TUnderlyingSystem>();
		builder.RedirectAsTransient<TI2, TUnderlyingSystem>();
		builder.RedirectAsTransient<TI3, TUnderlyingSystem>();
	}


	public static void SetupUnderlyingSystemSingleton<TUnderlyingSystem, TI1, TI2, TI3, TI4>(this IDependencyBuilder builder) where TUnderlyingSystem : TI1, TI2, TI3, TI4
	{
		builder.RemoveIfExists<TUnderlyingSystem>();
		builder.AddSingleton<TUnderlyingSystem>();

		builder.RedirectAsSingleton<TI1, TUnderlyingSystem>();
		builder.RedirectAsSingleton<TI2, TUnderlyingSystem>();
		builder.RedirectAsSingleton<TI3, TUnderlyingSystem>();
		builder.RedirectAsSingleton<TI4, TUnderlyingSystem>();
	}

	public static void SetupUnderlyingSystemScoped<TUnderlyingSystem, TI1, TI2, TI3, TI4>(this IDependencyBuilder builder) where TUnderlyingSystem : TI1, TI2, TI3, TI4
	{
		builder.RemoveIfExists<TUnderlyingSystem>();
		builder.AddScoped<TUnderlyingSystem>();

		builder.RedirectAsScoped<TI1, TUnderlyingSystem>();
		builder.RedirectAsScoped<TI2, TUnderlyingSystem>();
		builder.RedirectAsScoped<TI3, TUnderlyingSystem>();
		builder.RedirectAsScoped<TI4, TUnderlyingSystem>();
	}

	public static void SetupUnderlyingSystemTransient<TUnderlyingSystem, TI1, TI2, TI3, TI4>(this IDependencyBuilder builder) where TUnderlyingSystem : TI1, TI2, TI3, TI4
	{
		builder.RemoveIfExists<TUnderlyingSystem>();
		builder.AddTransient<TUnderlyingSystem>();

		builder.RedirectAsTransient<TI1, TUnderlyingSystem>();
		builder.RedirectAsTransient<TI2, TUnderlyingSystem>();
		builder.RedirectAsTransient<TI3, TUnderlyingSystem>();
		builder.RedirectAsTransient<TI4, TUnderlyingSystem>();
	}
}
