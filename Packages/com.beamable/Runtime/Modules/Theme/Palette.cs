using System;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Theme
{
	[Serializable]
	public abstract class GeneralPaletteBinding
	{
		public const string NAME_NONE = "<none>";

		public string Name;

		public bool HasName => !string.IsNullOrEmpty(Name) && !string.Equals(Name, NAME_NONE);

	}

	public static class GeneralPaletteBindingExtensions
	{
		public static bool Exists(this GeneralPaletteBinding binding)
		{
			return binding != null && binding.HasName;
		}

		public static T Clone<T>(this T binding)
		   where T : GeneralPaletteBinding, new()
		{
			return new T { Name = binding.Name };
		}
	}


	[Serializable]
	public abstract class PaletteBase
	{
		public abstract string[] StyleNames { get; }
		public abstract Type GetBindingType();


	}

	public abstract class PaletteStyle
	{
		[CanHide]
		public string Name;

		[CanHide]
		public bool Enabled;

		public virtual PaletteStyle Clone()
		{
			return MemberwiseClone() as PaletteStyle;
		}
	}


	[Serializable]
	public abstract class Palette<T> : PaletteBase where T : PaletteStyle
	{
		public List<T> Styles;

		public override string[] StyleNames => Styles?.Select(o => o.Name).ToArray() ?? new string[] { };

		public abstract T DefaultValue();

		public T Find(PaletteBinding binding)
		{
			return Find(binding.Name);
		}

		public T Find(string name, T defaultValue = default)
		{
			if (string.IsNullOrEmpty(name)) return defaultValue;

			var option = Styles?.FirstOrDefault(c => c.Enabled && c.Name.ToLower().Equals(name.ToLower()));
			return option ?? defaultValue;
		}

		public override Type GetBindingType()
		{
			return typeof(PaletteBinding);
		}

		[Serializable]
		public class PaletteBinding : GeneralPaletteBinding
		{
			public T Resolve()
			{
				return ThemeConfiguration.Instance.Style.GetPaletteStyle(this);
			}
		}

	}
}
