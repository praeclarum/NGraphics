using System;
using System.Globalization;

namespace XGraphics
{
	public struct Point
	{
		public double X;
		public double Y;
		public Point (double x, double y)
		{
			X = x;
			Y = y;
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
			return string.Format (CultureInfo.InvariantCulture, "[{0}, {1}]", X, Y);
		}
	}

	public struct Size
	{
		public Size (double width, double height)
		{
		}
		public Size (double size)
		{
		}
	}

	public struct Rectangle
	{
		public Rectangle (Point position, Size size)
		{
		}
	}
}

