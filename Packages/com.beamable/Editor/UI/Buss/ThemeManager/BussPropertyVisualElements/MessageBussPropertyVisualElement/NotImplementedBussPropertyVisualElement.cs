using Beamable.UI.Buss;

namespace Beamable.Editor.UI.Components
{
	public class NotImplementedBussPropertyVisualElement : MessageBussPropertyVisualElement
	{
		protected override string Message =>
			$"No visual element implemented for drawing a property of type {Property?.GetType().Name ?? "NULL"}.";

		public NotImplementedBussPropertyVisualElement(IBussProperty property) : base(property) { }
	}
}
