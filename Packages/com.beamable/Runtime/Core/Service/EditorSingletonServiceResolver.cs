namespace Beamable.Service
{
	public class EditorSingletonServiceResolver<TResolvedAs, TCreatedAs> : IServiceResolver<TResolvedAs>
	   where TResolvedAs : class
	   where TCreatedAs : class, TResolvedAs, new()
	{
		private static TCreatedAs instance;

		public bool CanResolve()
		{
			return true;
		}

		public bool Exists()
		{
			return instance != null;
		}

		public void OnTeardown()
		{
			instance = null;
		}

		public TResolvedAs Resolve()
		{
			if (instance == null)
			{
				instance = new TCreatedAs();
			}
			return instance;
		}
	}

	public class EditorSingletonServiceResolver<T> : EditorSingletonServiceResolver<T, T>
	   where T : class, new()
	{
	}
}
