using System;

namespace NGraphics
{
	public class Oval : IDrawable
	{
		Rectangle frame;
		Pen pen;
		Brush brush;

		public Oval (Rectangle frame, Pen pen = null, Brush brush = null)
		{
			this.frame = frame;
			this.pen = pen;
			this.brush = brush;
		}

		public Oval (Point position, Size size, Pen pen = null, Brush brush = null)
			: this (new Rectangle (position, size), pen, brush)
		{
		}

		public Oval (Point center, double radius)
			: this (center - radius, new Size (radius * 2))
		{
		}

		public void Draw (ISurface surface)
		{
			surface.DrawOval (frame, pen, brush);
		}
	}
}

