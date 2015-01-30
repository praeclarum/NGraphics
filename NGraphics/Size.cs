using System;
using System.Globalization;

namespace NGraphics
{
	public struct Size
	{
		public static readonly Size Zero = new Size ();

		public double Width;
		public double Height;

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
		public Size ()
		{
			Width = 0;
			Height = 0;
		}
	}	
}
