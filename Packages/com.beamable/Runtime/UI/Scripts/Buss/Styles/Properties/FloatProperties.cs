using System;
using UnityEngine;

namespace Beamable.UI.Buss
{
	[Serializable]
	public class FloatBussProperty : DefaultBussProperty, IFloatBussProperty, IFloatFromFloatBussProperty
	{
		[SerializeField]
		private float _floatValue;

		public float FloatValue
		{
			get => _floatValue;
			set => _floatValue = value;
		}

		public FloatBussProperty() { }

		public FloatBussProperty(float floatValue)
		{
			_floatValue = floatValue;
		}

		public float GetFloatValue(float input) => FloatValue;
		public IBussProperty CopyProperty()
		{
			return new FloatBussProperty(FloatValue);
		}

		public IBussProperty Interpolate(IBussProperty other, float value)
		{
			if (other is IFloatBussProperty fl)
			{
				return new FloatBussProperty(Mathf.Lerp(FloatValue, fl.FloatValue, value));
			}

			if (other is FractionFloatBussProperty frac)
			{
				return new FractionFloatBussProperty(Mathf.Lerp(0f, frac.Fraction, value), Mathf.Lerp(FloatValue, frac.Offset, value));
			}

			return CopyProperty();
		}
	}

	[Serializable]
	public class FractionFloatBussProperty : DefaultBussProperty, IFloatFromFloatBussProperty
	{
		public float Fraction;
		public float Offset;
		public FractionFloatBussProperty() { }

		public FractionFloatBussProperty(float fraction, float offset)
		{
			Fraction = fraction;
			Offset = offset;
		}

		public float GetFloatValue(float input)
		{
			return input * Fraction + Offset;
		}

		public IBussProperty CopyProperty()
		{
			return new FractionFloatBussProperty(Fraction, Offset);
		}

		public IBussProperty Interpolate(IBussProperty other, float value)
		{
			if (other is IFloatBussProperty fp)
			{
				return new FractionFloatBussProperty(Mathf.Lerp(Fraction, 0f, value), Mathf.Lerp(Offset, fp.FloatValue, value));
			}

			if (other is FractionFloatBussProperty frac)
			{
				return new FractionFloatBussProperty(Mathf.Lerp(Fraction, frac.Fraction, value), Mathf.Lerp(Offset, frac.Offset, value));
			}

			return CopyProperty();
		}
	}
}
