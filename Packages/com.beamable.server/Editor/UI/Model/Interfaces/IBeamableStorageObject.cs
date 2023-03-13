using Beamable.Server.Editor;

namespace Beamable.Editor.UI.Model
{
	public interface IBeamableStorageObject : IBeamableService
	{
		StorageObjectDescriptor ServiceDescriptor { get; }
		MongoStorageBuilder ServiceBuilder { get; }
	}
}
