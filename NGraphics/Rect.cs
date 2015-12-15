using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	public struct Rect
	{
		public double X;
		public double Y;
		public double Width;
		public double Height;

		[System.Runtime.Serialization.IgnoreDataMember]
		public double Left { get { return X; } }
		[System.Runtime.Serialization.IgnoreDataMember]
		public double Top { get { return Y; } }
		[System.Runtime.Serialization.IgnoreDataMember]
		public double Right { get { return X + Width; } }
		[System.Runtime.Serialization.IgnoreDataMember]
		public double Bottom { get { return Y + Height; } }

		[System.Runtime.Serialization.IgnoreDataMember]
		public Point Position { get { return new Point (X, Y); } }
		[System.Runtime.Serialization.IgnoreDataMember]
		public Size Size { get { return new Size (Width, Height); } }

		[System.Runtime.Serialization.IgnoreDataMember]
		public Point TopLeft { get { return new Point (X, Y); } }
		[System.Runtime.Serialization.IgnoreDataMember]
		public Point BottomLeft { get { return new Point (X, Y + Height); } }
		[System.Runtime.Serialization.IgnoreDataMember]
		public Point Center { get { return new Point (X + Width/2, Y + Height/2); } }
		[System.Runtime.Serialization.IgnoreDataMember]
		public Point TopRight { get { return new Point (X + Width, Y); } }
		[System.Runtime.Serialization.IgnoreDataMember]
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

		public override bool Equals (object obj)
		{
			if (obj is Rect) {
				return this == ((Rect)obj);
			}
			return false;
		}

		public override int GetHashCode ()
		{
			return X.GetHashCode () * 5 + Y.GetHashCode () * 13 + Width.GetHashCode () * 19 + Height.GetHashCode () * 29;
		}

		public static bool operator == (Rect a, Rect b)
		{
			return a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
		}

		public static bool operator != (Rect a, Rect b)
		{
			return a.X != b.X || a.Y != b.Y || a.Width != b.Width || a.Height == b.Height;
		}

		public static Rect operator + (Rect a, Point offset)
		{
			return new Rect (a.X + offset.X, a.Y + offset.Y, a.Width, a.Height);
		}
		public static Rect operator - (Rect a, Point offset)
		{
			return new Rect (a.X - offset.X, a.Y - offset.Y, a.Width, a.Height);
		}

		public static Rect operator + (Rect a, Size offset)
		{
			return new Rect (a.X + offset.Width, a.Y + offset.Height, a.Width, a.Height);
		}
		public static Rect operator - (Rect a, Size offset)
		{
			return new Rect (a.X - offset.Width, a.Y - offset.Height, a.Width, a.Height);
		}

		public static Rect operator * (Rect a, Size s)
		{
			return new Rect (a.X * s.Width, a.Y * s.Height, a.Width * s.Width, a.Height * s.Height);
		}
		public static Rect operator * (Size s, Rect a)
		{
			return new Rect (a.X * s.Width, a.Y * s.Height, a.Width * s.Width, a.Height * s.Height);
		}

		public Rect GetInflated (double dx, double dy)
		{
			var r = this;
			r.Inflate (dx, dy);
			return r;
		}

		public Rect GetInflated (double d)
		{
			var r = this;
			r.Inflate (d, d);
			return r;
		}

		public Rect GetInflated (Size padding)
		{
			return GetInflated (padding.Width, padding.Height);
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

		public Rect Union (Rect p)
		{
			var x = Math.Min (p.X, X);
			var y = Math.Min (p.Y, Y);
			var r = Math.Max (p.Right, Right);
			var b = Math.Max (p.Bottom, Bottom);
			return new Rect (x, y, r - x, b - y);
		}

		public Rect Union (Point p)
		{
			var x = Math.Min (p.X, X);
			var y = Math.Min (p.Y, Y);
			var r = Math.Max (p.X, Right);
			var b = Math.Max (p.Y, Bottom);
			return new Rect (x, y, r - x, b - y);
		}

		public static Rect Union (IEnumerable<Rect> rects)
		{
			var res = new Rect ();
			var hasr = false;
			foreach (var r in rects) {
				if (hasr) {
					res = res.Union (r);
				} else {
					res = r;
					hasr = true;
				}
			}
			return res;
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

		public bool Contains (Point point)
		{
			return ((X <= point.X && point.X <= Right) &&
					(Y <= point.Y && point.Y <= Bottom));
		}

		public bool Intersects (Rect other)
		{
			return ((Right >= other.Left && Left <= other.Right) &&
					(Bottom >= other.Top && Top <= other.Bottom));
		}

		public double DistanceTo (Point point)
		{
			throw new NotImplementedException ();
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
		public void Add (IEnumerable<Point> points)
		{
			foreach (var p in points) {
				Add (p);
			}
		}
	}
}
