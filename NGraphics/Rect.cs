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

		public Point TopLeft { get { return new Point (X, Y); } }
		public Point BottomLeft { get { return new Point (X, Y + Height); } }
		public Point Center { get { return new Point (X + Width/2, Y + Height/2); } }
		public Point TopRight { get { return new Point (X + Width, Y); } }
		public Point BottomRight { get { return new Point (X + Width, Y + Height); } }

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

		public void Inflate (Size padding)
		{
			Inflate (padding.Width, padding.Height);
		}

		public void Inflate (double dx, double dy)
		{
			X -= dx;
			Y -= dy;
			Width += 2*dx;
			Height += 2*dy;
		}

		public Rect Union (Point p)
		{
			var x = Math.Min (p.X, X);
			var y = Math.Min (p.Y, Y);
			var r = Math.Max (p.X, Right);
			var b = Math.Max (p.Y, Bottom);
			return new Rect (x, y, r - x, b - y);
		}

		public Rect MoveInto (Rect frame)
		{
			var r = this;
			if (r.Right > frame.Right)
				r.X = frame.Right - r.Width;
			if (r.X < frame.X) r.X = frame.X;
			if (r.Bottom > frame.Bottom)
				r.Y = frame.Bottom - r.Height;
			if (r.Y < frame.Y) r.Y = frame.Y;
			return r;
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Rect ({0}, {1}, {2}, {3})", X, Y, Width, Height);
		}
	}

	public class BoundingBoxBuilder
	{
		Rect bb;
		int nbb = 0;
		public Rect BoundingBox { get { return bb; } }
		public void Add (Point point)
		{
			if (nbb == 0) {
				bb = new Rect (point, Size.Zero);
			}
			else {
				bb = bb.Union (point);
			}
			nbb++;
		}
	}
}
