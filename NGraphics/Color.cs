using System;

namespace NGraphics
{
	public struct Color
	{
		public readonly double Red;
		public readonly double Green;
		public readonly double Blue;
		public readonly double Alpha;

		public int RedByte { get { return (int)(Red * 255 + 0.5); } }
		public int GreenByte { get { return (int)(Green * 255 + 0.5); } }
		public int BlueByte { get { return (int)(Blue * 255 + 0.5); } }
		public int AlphaByte { get { return (int)(Alpha * 255 + 0.5); } }

		public Color (double red, double green, double blue, double alpha)
		{
			Red = red;
			Green = green;
			Blue = blue;
			Alpha = alpha;
		}

		public Color (double white, double alpha)
		{
			Red = white;
			Green = white;
			Blue = white;
			Alpha = alpha;
		}
	}

	public static class Colors
	{
		public static readonly Color Black = new Color (0, 0, 0, 1);
		public static readonly Color White = new Color (1, 1, 1, 1);
		public static readonly Color Red = new Color (1, 0, 0, 1);
		public static readonly Color Yellow = new Color (1, 1, 0, 1);
	}
}

