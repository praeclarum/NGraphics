using System;
using System.Globalization;

namespace NGraphics
{
	public struct Size
	{
		public static readonly Size Zero = new Size ();

		public double Width;
		public double Height;

		public double Max { get { return Math.Max (Width, Height); } }
		public double Min { get { return Math.Min (Width, Height); } }

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

		public static Size operator * (Size a, double v)
		{
			return new Size (a.Width * v, a.Height * v);
		}
		public static Size operator * (double v, Size a)
		{
			return new Size (a.Width * v, a.Height * v);
		}
		public static Size operator / (Size a, double v)
		{
			return new Size (a.Width / v, a.Height / v);
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Size ({0}, {1})", Width, Height);
		}
	}	
}
