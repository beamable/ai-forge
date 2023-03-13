using System;
using UnityEngine;

namespace Beamable.UI.Buss
{
	[Serializable]
	public class Vector2BussProperty : DefaultBussProperty, IVector2BussProperty
	{
		[SerializeField]
		private Vector2 _vector2Value;

		public Vector2 Vector2Value
		{
			get => _vector2Value;
			set => _vector2Value = value;
		}

		public Vector2BussProperty() { }

		public Vector2BussProperty(Vector2 vector2Value)
		{
			_vector2Value = vector2Value;
		}

		public IBussProperty CopyProperty()
		{
			return new Vector2BussProperty(_vector2Value);
		}
	}
}
