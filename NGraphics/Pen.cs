using System;

namespace NGraphics
{
	public static class Pens
	{
		public static readonly Pen Black = new Pen (Colors.Black, 1);
		public static readonly Pen Red = new Pen (Colors.Red, 1);
	}

	public class Pen
	{
		public readonly Color Color;
		public readonly double Width;

		public Pen (Color color, double width)
		{
			Color = color;
			Width = width;
		}

		public Pen WithWidth (double width)
		{
			return new Pen (Color, width);
		}

		public Pen WithColor (Color color)
		{
			return new Pen (color, Width);
		}
	}
}

