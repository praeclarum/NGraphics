using System;
using System.Globalization;

namespace NGraphics
{
	public struct Rectangle
	{
		public double X;
		public double Y;
		public double Width;
		public double Height;

		public Rectangle (double x, double y, double width, double height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public Rectangle (Point position, Size size)
			: this (position.X, position.Y, size.Width, size.Height)
		{
		}

		public Rectangle (Size size)
			: this (0, 0, size.Width, size.Height)
		{
		}
	}
}
