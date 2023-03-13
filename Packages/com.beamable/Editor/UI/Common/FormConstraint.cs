using System;
using System.Collections.Generic;

namespace Beamable.Editor.UI.Common
{
	public delegate bool FormErrorCheck(out string error);
	public delegate string FormErrorCheckWithInput(string field);
	public delegate string FormBoolErrorCheckWithInput(bool field);

	public class FormConstraint
	{

		public static FormConstraint Logical(string name, Func<bool> checker)
		{
			var constraint = new FormConstraint()
			{
				Name = name,
				ErrorCheck = (out string err) =>
				{
					err = null;
					return checker();
				}
			};
			constraint.Check();

			return constraint;
		}

		public FormErrorCheck ErrorCheck;
		public string Name;
		public event Action<bool> OnValidate;
		public event Action OnNotify;

		public void Check(bool isForceCheck = false)
		{
			OnValidate?.Invoke(isForceCheck);

		}
		public void Notify()
		{
			OnNotify?.Invoke();
		}

		public bool IsValid => !ErrorCheck(out string err);

	}

	public class FormConstraintCollection
	{
		public List<FormConstraint> Constraints;

	}
}
