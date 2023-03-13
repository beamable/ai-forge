namespace Beamable.Editor.UI.Components
{
	public class CustomMessageBussPropertyVisualElement : MessageBussPropertyVisualElement
	{
		protected override string Message { get; }

		public CustomMessageBussPropertyVisualElement(string text) : base(null)
		{
			Message = text;
		}
	}
}
