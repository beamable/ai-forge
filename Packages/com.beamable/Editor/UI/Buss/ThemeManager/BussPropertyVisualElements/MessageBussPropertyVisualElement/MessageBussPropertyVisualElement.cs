using Beamable.UI.Buss;
using UnityEngine.UIElements;

namespace Beamable.Editor.UI.Components
{
	public abstract class MessageBussPropertyVisualElement : BussPropertyVisualElement<IBussProperty>
	{
		protected abstract string Message
		{
			get;
		}

		protected MessageBussPropertyVisualElement(IBussProperty property) : base(property) { }

		public override void Init()
		{
			base.Init();
			var label = new Label(Message);
			AddBussPropertyFieldClass(label);
			label.AddTextWrapStyle();
			Root.Add(label);
		}

		public override void OnPropertyChangedExternally()
		{

		}
	}
}
