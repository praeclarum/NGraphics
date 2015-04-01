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
		public static readonly Point OneX = new Point (1, 0);
		public static readonly Point OneY = new Point (0, 1);

		public double X;
		public double Y;

		public Point (double x, double y)
		{
			X = x;
			Y = y;
		}

		public Point WithX (double x) {
			return new Point (x, Y);
		}
		public Point WithY (double y) {
			return new Point (X, y);
		}

		public Point ReflectedAround (Point point)
		{
			// this = point + d
			// this' = point - d
			var d = this - point;
			return point - d;
		}

		public double Distance {
			get { return Math.Sqrt (X * X + Y * Y); }
		}

		public override bool Equals (object obj)
		{
			if (obj is Point) {
				return this == (Point)obj;
			}
			return false;
		}

		public override int GetHashCode ()
		{
			return 5 * X.GetHashCode () + 11 * Y.GetHashCode ();
		}

		public static bool operator == (Point a, Point b)
		{
			return a.X == b.X && a.Y == b.Y;
		}
		public static bool operator != (Point a, Point b)
		{
			return a.X != b.X || a.Y != b.Y;
		}

		public static Point operator * (Point a, Size v)
		{
			return new Point (a.X * v.Width, a.Y * v.Height);
		}
		public static Point operator * (Size v, Point a)
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
		public static Point operator * (double v, Point a)
		{
			return new Point (a.X * v, a.Y * v);
		}

		public static Point operator - (Point a)
		{
			return new Point (-a.X, -a.Y);
		}
		public static Point operator - (Point a, Point v)
		{
			return new Point (a.X - v.X, a.Y - v.Y);
		}
		public static Point operator + (Point a, Point v)
		{
			return new Point (a.X + v.X, a.Y + v.Y);
		}

		public static Point operator - (Point a, Size v)
		{
			return new Point (a.X - v.Width, a.Y - v.Height);
		}
		public static Point operator + (Point a, Size v)
		{
			return new Point (a.X + v.Width, a.Y + v.Height);
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

