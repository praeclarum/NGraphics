using System;
using System.Collections.Generic;

namespace NGraphics
{
	public static class Pens
	{
		public static readonly Pen Black = new Pen (Colors.Black, 1);
		public static readonly Pen LightGray = new Pen (Colors.LightGray, 1);
		public static readonly Pen Gray = new Pen (Colors.Gray, 1);
		public static readonly Pen DarkGray = new Pen (Colors.DarkGray, 1);
		public static readonly Pen White = new Pen (Colors.White, 1);
		public static readonly Pen Red = new Pen (Colors.Red, 1);
		public static readonly Pen Yellow = new Pen (Colors.Yellow, 1);
		public static readonly Pen Green = new Pen (Colors.Green, 1);
		public static readonly Pen Blue = new Pen (Colors.Blue, 1);
	}

	public class Pen
	{
		public Color Color;
		public double Width;
        public IEnumerable<float> DashPattern; 

		public Pen ()
		{
			Color = Colors.Black;
			Width = 1;
		}

		public Pen (Color color, double width = 1.0)
		{
			Color = color;
			Width = width;
		}
		public Pen (string colorString, double width = 1.0)
			: this (new Color (colorString), width)
		{
		}

		public Pen WithWidth (double width)
		{
			return new Pen (Color, width);
		}

		public Pen WithColor (Color color)
		{
			return new Pen (color, Width);
		}

		public override string ToString ()
		{
			return string.Format ("Pen ({0}, {1})", Color, Width);
		}
	}
}

