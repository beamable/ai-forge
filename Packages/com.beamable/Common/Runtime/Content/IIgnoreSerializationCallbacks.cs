namespace Beamable.Content
{
	/// <summary>
	/// When content is serialized, any type that inherits from this interface will not have their Unity
	/// serialization callback methods during for the content serialization.
	/// </summary>
	public interface IIgnoreSerializationCallbacks { }
}
