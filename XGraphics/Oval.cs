using System;

namespace XGraphics
{
	public class Oval : IDrawable
	{
		Point position;
		Size size;
		Pen pen;
		Brush brush;

		public Oval (Point position, Size size, Pen pen = null, Brush brush = null)
		{
			this.position = position;
			this.size = size;
			this.pen = pen;
			this.brush = brush;
		}

		public Oval (Point center, double radius)
			: this (center - radius, new Size (radius * 2))
		{
		}

		public void Draw (ISurface surface)
		{
			surface.DrawOval (position, size, pen, brush);
		}
	}
}

