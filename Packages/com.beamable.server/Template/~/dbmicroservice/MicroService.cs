using System;
using System.Reflection;

namespace Beamable.Server
{
	public delegate TMicroService ServiceFactory<out TMicroService>() where TMicroService : MicroService;

	public abstract class MicroService
	{
		protected RequestContext Context;

		internal void ProvideContext(RequestContext ctx)
		{
			Context = ctx;
		}
	}
}
