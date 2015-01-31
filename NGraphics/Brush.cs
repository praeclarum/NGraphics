using System;
using System.Collections.Generic;

namespace NGraphics
{
	public abstract class Brush
	{
	}

	public static class Brushes
	{
		public static readonly SolidBrush Black = new SolidBrush (Colors.Black);
		public static readonly SolidBrush Gray = new SolidBrush (Colors.Gray);
		public static readonly SolidBrush White = new SolidBrush (Colors.White);
		public static readonly SolidBrush Red = new SolidBrush (Colors.Red);
		public static readonly SolidBrush Yellow = new SolidBrush (Colors.Yellow);
		public static readonly SolidBrush Green = new SolidBrush (Colors.Green);
		public static readonly SolidBrush Blue = new SolidBrush (Colors.Blue);
	}

	public class SolidBrush : Brush
	{
		public Color Color;

		public SolidBrush ()
		{
		}

		public SolidBrush (Color color)
		{
			Color = color;
		}
	}

	public class GradientStop
	{
		public double Offset;
		public Color Color;
	}

	public class RadialGradientBrush : Brush
	{
		public Point RelativeCenter;
		public Point RelativeFocus;
		public double RelativeRadius;
		public readonly List<GradientStop> Stops = new List<GradientStop> ();
	}

	public class LinearGradientBrush : Brush
	{
		public Point RelativeStart;
		public Point RelativeEnd;
		public readonly List<GradientStop> Stops = new List<GradientStop> ();
	}
}
