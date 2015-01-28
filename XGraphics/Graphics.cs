using System;

namespace XGraphics
{
	public class Graphic
	{
		public Graphic ()
		{
		}
	}

	public class Oval : Graphic
	{
		public Oval (Point position, Size size)
		{
		}

		public Oval (Point center, double radius)
			: this (center - radius, new Size (radius * 2))
		{
		}
	}
}

