namespace Beamable.Editor.UI.Validation
{
	public abstract class ValidationRule<T>
	{
		public abstract string ErrorMessage { get; }
		public bool Satisfied { get; protected set; }
		protected string ComponentName { get; }

		public abstract void Validate(T value);

		protected ValidationRule(string componentName)
		{
			ComponentName = componentName;
		}
	}
}
