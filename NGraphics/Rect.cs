using System;
using System.Globalization;

namespace NGraphics
{
	public struct Rect
	{
		public double X;
		public double Y;
		public double Width;
		public double Height;

		public double Right { get { return X + Width; } }
		public double Bottom { get { return Y + Height; } }

		public Point Position { get { return new Point (X, Y); } }
		public Size Size { get { return new Size (Width, Height); } }

		public Rect (double x, double y, double width, double height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}
		public Rect (Point position, Size size)
			: this (position.X, position.Y, size.Width, size.Height)
		{
		}
		public Rect (Size size)
			: this (0, 0, size.Width, size.Height)
		{
		}

		public Rect Union (Point p)
		{
			var x = Math.Min (p.X, X);
			var y = Math.Min (p.Y, Y);
			var r = Math.Max (p.X, Right);
			var b = Math.Max (p.Y, Bottom);
			return new Rect (x, y, r - x, b - y);
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Rect ({0}, {1}, {2}, {3})", X, Y, Width, Height);
		}
	}
}
