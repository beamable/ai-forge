using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.ToolbarExtender
{
	/// <summary>
	/// Scriptable object that gets loaded from <see cref="CoreConfiguration.BeamableAssistantToolbarButtonsPaths"/>s when the <see cref="BeamableToolbarExtender"/> initializes
	/// or one of these is re-imported.
	///
	/// <para/>
	///
	/// These can be used to add buttons to Unity's play-mode toolbar, either to the left or right of them (see <see cref="BeamableToolbarExtender"/>).
	/// </summary>
	public abstract class BeamableToolbarButton : ScriptableObject
	{
		public enum Side
		{
			Left,
			Right
		}

		[SerializeField] private Vector2 ButtonSize = Vector2.one * 30;
		[SerializeField] private Texture ButtonImage = null;
		[SerializeField] private string ButtonText = "Button";

		[SerializeField] private bool IsVisible = true;
		[SerializeField] private int Order = 0;
		[SerializeField] private Side Location = Side.Left;

		public abstract void OnButtonClicked(BeamEditorContext editorAPI);

		public virtual bool ShouldDisplayButton(BeamEditorContext editorAPI) => IsVisible;
		public virtual Side GetButtonSide(BeamEditorContext editorAPI) => Location;

		public virtual int GetButtonOrder(BeamEditorContext editorAPI) => Order;
		public virtual string GetButtonText(BeamEditorContext editorAPI) => ButtonText;
		public virtual Vector2 GetButtonSize(BeamEditorContext editorAPI) => ButtonSize;
		public virtual Texture GetButtonTexture(BeamEditorContext editorAPI) => ButtonImage;

		/// <summary>
		/// Override when you want to make a dropdown menu.
		/// </summary>
		/// <param name="editorAPI">The current initialized <see cref="BeamEditorContext"/> instance.</param>
		/// <returns>A built generic menu to be displayed when the button is clicked. Or null, if you want the button to behave like a regular button.</returns>
		public virtual GenericMenu GetDropdownOptions(BeamEditorContext editorAPI) => null;
	}
}
