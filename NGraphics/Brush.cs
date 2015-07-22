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
		public void AddStops (IEnumerable<GradientStop> stops)
		{
			Stops.AddRange(stops);
		}
	}

	public class RadialGradientBrush : GradientBrush
	{
		public Point Center;
		public Point Focus;
		public Size Radius;
		public bool Absolute = false;

		public RadialGradientBrush ()
		{
		}
		public RadialGradientBrush (Point relCenter, Size relRadius, params GradientStop[] stops)
		{
			Center = relCenter;
			Focus = relCenter;
			Radius = relRadius;
			Stops.AddRange (stops);
		}
		public RadialGradientBrush (Point relCenter, Size relRadius, Color startColor, Color endColor)
		{
			Center = relCenter;
			Focus = relCenter;
			Radius = relRadius;
			Stops.Add (new GradientStop (0, startColor));
			Stops.Add (new GradientStop (1, endColor));
		}
		public RadialGradientBrush (Color startColor, Color endColor)
			: this (new Point (0.5, 0.5), new Size (0.5), startColor, endColor)
		{
		}
		public RadialGradientBrush (Point relCenter, Size relRadius, Color startColor, Color midColor, Color endColor)
		{
			Center = relCenter;
			Focus = relCenter;
			Radius = relRadius;
			Stops.Add (new GradientStop (0, startColor));
			Stops.Add (new GradientStop (0.5, midColor));
			Stops.Add (new GradientStop (1, endColor));
		}
		public RadialGradientBrush (Color startColor, Color midColor, Color endColor)
			: this (new Point (0.5, 0.5), new Size (0.5), startColor, midColor, endColor)
		{
		}
		public Point GetAbsoluteCenter (Rect frame)
		{
			if (Absolute) return Center;
			return frame.TopLeft + Center * frame.Size;
		}
		public Size GetAbsoluteRadius (Rect frame)
		{
			if (Absolute) return Radius;
			return Radius * frame.Size;
		}
		public Point GetAbsoluteFocus (Rect frame)
		{
			if (Absolute) return Focus;
			return frame.TopLeft + Focus * frame.Size;
		}
	}

	public class LinearGradientBrush : GradientBrush
	{
		public Point Start;
		public Point End;
		public bool Absolute = false;

		public LinearGradientBrush ()
		{
		}
		public LinearGradientBrush (Point relStart, Point relEnd, params GradientStop[] stops)
		{
			Start = relStart;
			End = relEnd;
			Stops.AddRange (stops);
		}
		public LinearGradientBrush (Point relStart, Point relEnd, Color startColor, Color endColor)
		{
			Start = relStart;
			End = relEnd;
			Stops.Add (new GradientStop (0, startColor));
			Stops.Add (new GradientStop (1, endColor));
		}
		public LinearGradientBrush (Point relStart, Point relEnd, Color startColor, Color midColor, Color endColor)
		{
			Start = relStart;
			End = relEnd;
			Stops.Add (new GradientStop (0, startColor));
			Stops.Add (new GradientStop (0.5, midColor));
			Stops.Add (new GradientStop (1, endColor));
		}
		public Point GetAbsoluteStart (Rect frame)
		{
			if (Absolute) return Start;
			return frame.TopLeft + Start * frame.Size;
		}
		public Point GetAbsoluteEnd (Rect frame)
		{
			if (Absolute) return End;
			return frame.TopLeft + End * frame.Size;
		}
	}
}
