using System;
using System.Globalization;

namespace NGraphics
{
	/// <summary>
	/// Yet another 2D point struct. I hope you enjoy this one.
	/// </summary>
	public struct Point
	{
		public static readonly Point Zero = new Point (0, 0);

		public double X;
		public double Y;

		public Point (double x, double y)
		{
			X = x;
			Y = y;
		}

		public Point ReflectedAround (Point point)
		{
			// this = point + d
			// this' = point - d
			var d = this - point;
			return point - d;
		}

		public static Point operator * (Point a, Size v)
		{
			return new Point (a.X * v.Width, a.Y * v.Height);
		}
		public static Point operator / (Point a, Size v)
		{
			return new Point (a.X / v.Width, a.Y / v.Height);
		}

		public static Point operator * (Point a, double v)
		{
			return new Point (a.X * v, a.Y * v);
		}
		public static Point operator / (Point a, double v)
		{
			return new Point (a.X / v, a.Y / v);
		}

		public static Point operator - (Point a, Point v)
		{
			return new Point (a.X - v.X, a.Y - v.Y);
		}
		public static Point operator + (Point a, Point v)
		{
			return new Point (a.X + v.X, a.Y + v.Y);
		}

		public static Point operator - (Point a, double v)
		{
			return new Point (a.X - v, a.Y - v);
		}
		public static Point operator + (Point a, double v)
		{
			return new Point (a.X + v, a.Y + v);
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Point ({0}, {1})", X, Y);
		}
	}
}

