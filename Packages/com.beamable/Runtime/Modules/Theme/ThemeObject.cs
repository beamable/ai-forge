using Beamable.Theme.Palettes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Theme
{
	public class ThemeObject : ScriptableObject
	{
		public ThemeObject Parent;
		public ColorPalette ColorPalette;
		public TextPalette TextPalette;
		public FontPalette FontPalette;
		public ImagePalette ImagePalette;
		public GradientPalette GradientPalette;
		public LayoutPalette LayoutPalette;
		public SelectablePalette SelectablePalette;
		public ButtonPalette ButtonPalette;
		public TransformPalette TransformPalette;
		public SoundPalette SoundPalette;
		public WindowPalette WindowPalette;
		public StringPalette StringPalette;

		public int Version { get; private set; }

		public string Hash
		{
			get
			{
				var parentHash = Parent == null ? "root" : Parent.Hash;
				return $"{parentHash}.{Version}";
			}
		}

		public PaletteStyleCopier<T> CloneParentPaletteStyle<T>(Palette<T>.PaletteBinding binding)
		   where T : PaletteStyle
		{
			var palette = GetPaletteByBindingType(binding.GetType()) as Palette<T>;
			var style = palette.Find(binding);

			// if the binding exists in the current theme, then we return that, but marked with a CANT-COPY status
			if (style != null)
			{
				return new PaletteStyleCopier<T>(palette, style, false);
			}

			// the binding doesn't exist, then we clone the value from the parent...
			var parentValue = Parent.GetPaletteStyle(binding).Clone() as T;
			return new PaletteStyleCopier<T>(palette, parentValue, true);
		}

		public T GetPaletteStyle<T>(Palette<T>.PaletteBinding binding, bool ascendThemeTree = true)
		   where T : PaletteStyle
		{
			if (binding == null)
			{
				return GetPaletteByType<T>().DefaultValue();
			}

			var palette = GetPaletteByBindingType(binding.GetType()) as Palette<T>;
			if (palette == null && Parent != null)
			{
				var parentValue = Parent.GetPaletteStyle(binding);
				return parentValue;
			}

			var option = palette.Find(binding);
			if (option != null)
			{
				return option;
			}
			if (Parent != null && ascendThemeTree)
			{
				var parentValue = Parent.GetPaletteStyle(binding, ascendThemeTree);
				return parentValue;
			}

			return GetPaletteByType<T>().DefaultValue();
		}

		public List<string> GetPaletteStyleNames(Type bindingType)
		{
			var existingSet = new HashSet<string>();
			if (Parent != null)
			{
				foreach (var optionName in Parent.GetPaletteStyleNames(bindingType))
				{
					existingSet.Add(optionName);
				}
			}
			var palette = GetPaletteByBindingType(bindingType);
			foreach (var optionName in palette.StyleNames)
			{
				existingSet.Add(optionName);
			}

			return existingSet.ToList();
		}

		public void BumpVersion()
		{
			Version += 1;
		}

		private void OnValidate()
		{
			BumpVersion();
		}

		private PaletteBase[] AllPalettes => new PaletteBase[]
		{
		 ColorPalette, TextPalette, FontPalette, ImagePalette, GradientPalette, LayoutPalette, SelectablePalette, ButtonPalette, TransformPalette, WindowPalette, SoundPalette, StringPalette
		};

		private PaletteBase GetPaletteByBindingType(Type bindingType)
		{
			foreach (var palette in AllPalettes)
			{
				if (palette.GetBindingType().IsAssignableFrom(bindingType))
				{
					return palette;
				}
			}
			throw new Exception("Unknown palette type");
		}

		private Palette<T> GetPaletteByType<T>()
		   where T : PaletteStyle
		{
			return GetPaletteByBindingType(typeof(Palette<T>.PaletteBinding)) as Palette<T>;
		}
	}


	[System.Serializable]
	public enum StyleBlendMode
	{
		Multiply, Additive, Override, Ignore
	}

	public static class StyleBlendModeExtensions
	{
		public static Color Blend(this StyleBlendMode blendMode, Color a, Color b)
		{
			switch (blendMode)
			{
				case StyleBlendMode.Additive:
					return a + b;
				case StyleBlendMode.Multiply:
					return a * b;
				case StyleBlendMode.Override:
					return b;
				default:
					return a;
			}
		}
	}

}
