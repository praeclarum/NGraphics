using System;
using System.Globalization;

namespace NGraphics
{
	public struct Size
	{
		public static readonly Size Zero = new Size ();
		public static readonly Size One = new Size (1);

		public static readonly Size MinValue = new Size (double.MinValue, double.MinValue);
		public static readonly Size MaxValue = new Size (double.MaxValue, double.MaxValue);

		public double Width;
		public double Height;

		public double Max { get { return Math.Max (Width, Height); } }
		public double Min { get { return Math.Min (Width, Height); } }

		public double Diagonal { get { return Math.Sqrt (Width*Width + Height*Height); } }

		public Point Center { get { return new Point (Width * 0.5, Height * 0.5); } }

		public Size (double width, double height)
		{
			Width = width;
			Height = height;
		}
		public Size (double size)
		{
			Width = size;
			Height = size;
		}

		public override bool Equals (object obj)
		{
			if (obj is Size) {
				var e = this == ((Size)obj);
				return e;
			}
			return false;
		}

		public override int GetHashCode ()
		{
			return Width.GetHashCode () * 5 + Height.GetHashCode () * 13;
		}

		public static bool operator == (Size a, Size b)
		{
			return a.Width == b.Width && a.Height == b.Height;
		}

		public static bool operator != (Size a, Size b)
		{
			return a.Width != b.Width || a.Height != b.Height;
		}

		public static Size operator - (Size a)
		{
			return new Size (-a.Width, -a.Height);
		}

		public static Size operator + (Size a, Size b)
		{
			return new Size (a.Width + b.Width, a.Height + b.Height);
		}
		public static Size operator - (Size a, Size b)
		{
			return new Size (a.Width - b.Width, a.Height - b.Height);
		}
		public static Size operator + (Size a, Point b)
		{
			return new Size (a.Width + b.X, a.Height + b.Y);
		}
		public static Size operator - (Size a, Point b)
		{
			return new Size (a.Width - b.X, a.Height - b.Y);
		}

		public static Size operator * (Size a, Size b)
		{
			return new Size (a.Width * b.Width, a.Height * b.Height);
		}
		public static Size operator * (Size a, double v)
		{
			return new Size (a.Width * v, a.Height * v);
		}
		public static Size operator * (double v, Size a)
		{
			return new Size (a.Width * v, a.Height * v);
		}
		public static Size operator / (Size a, Size b)
		{
			return new Size (a.Width / b.Width, a.Height / b.Height);
		}
		public static Size operator / (Size a, double v)
		{
			return new Size (a.Width / v, a.Height / v);
		}
		public static Size operator / (double v, Size a)
		{
			return new Size (v / a.Width, v / a.Height);
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Size ({0}, {1})", Width, Height);
		}
	}	
}
