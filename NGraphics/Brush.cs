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
		public static readonly SolidBrush DarkGray = new SolidBrush (Colors.DarkGray);
		public static readonly SolidBrush Gray = new SolidBrush (Colors.Gray);
		public static readonly SolidBrush LightGray = new SolidBrush (Colors.LightGray);
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
			Color = Colors.Black;
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
		public GradientStop ()
		{			
		}
		public GradientStop (double offset, Color color)
		{
			Offset = offset;
			Color = color;
		}
	}

	public abstract class GradientBrush : Brush
	{
		public readonly List<GradientStop> Stops = new List<GradientStop> ();
		public void AddStop (double offset, Color color)
		{
			Stops.Add (new GradientStop (offset, color));
		}
	}

	public class RadialGradientBrush : GradientBrush
	{
		public Point RelativeCenter;
		public Point RelativeFocus;
		public double RelativeRadius;

		public RadialGradientBrush ()
		{
		}
		public RadialGradientBrush (Point relCenter, double relRadius, params GradientStop[] stops)
		{
			RelativeCenter = relCenter;
			RelativeFocus = relCenter;
			RelativeRadius = relRadius;
			Stops.AddRange (stops);
		}
	}

	public class LinearGradientBrush : GradientBrush
	{
		public Point RelativeStart;
		public Point RelativeEnd;

		public LinearGradientBrush ()
		{
		}
		public LinearGradientBrush (Point relStart, Point relEnd, params GradientStop[] stops)
		{
			RelativeStart = relStart;
			RelativeEnd = relEnd;
			Stops.AddRange (stops);
		}
		public LinearGradientBrush (Point relStart, Point relEnd, Color startColor, Color endColor)
		{
			RelativeStart = relStart;
			RelativeEnd = relEnd;
			Stops.Add (new GradientStop (0, startColor));
			Stops.Add (new GradientStop (1, endColor));
		}
	}
}
