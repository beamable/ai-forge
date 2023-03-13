namespace Beamable.Theme
{
	public interface IPaletteStyleCopier
	{
		void Commit();
		PaletteStyle GetStyle();
	}
	public class PaletteStyleCopier<T> : IPaletteStyleCopier where T : PaletteStyle
	{
		public T Style { get; private set; }
		private Palette<T> _palette;
		private bool _canCopy;

		public PaletteStyleCopier(Palette<T> palette, T style, bool canCopy)
		{
			Style = style;
			_canCopy = canCopy;
			_palette = palette;
		}

		public void Commit()
		{
			if (_canCopy && !_palette.Styles.Contains(Style))
			{
				_palette.Styles.Add(Style);
			}
		}

		public PaletteStyle GetStyle()
		{
			return Style;
		}
	}
}
