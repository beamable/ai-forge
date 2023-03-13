namespace Beamable.Common.Dependencies
{
	public interface IDependencyNameProvider
	{
		string DependencyProviderName { get; }
	}

	public interface IDependencyScopeNameProvider
	{
		string DependencyScopeName { get; }
	}
}
