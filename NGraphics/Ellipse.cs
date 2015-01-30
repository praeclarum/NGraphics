using System;

namespace NGraphics
{
	public class Ellipse : IDrawable
	{
		Rectangle frame;
		Pen pen;
		Brush brush;

		public Ellipse (Rectangle frame, Pen pen = null, Brush brush = null)
		{
			this.frame = frame;
			this.pen = pen;
			this.brush = brush;
		}

		public Ellipse (Point position, Size size, Pen pen = null, Brush brush = null)
			: this (new Rectangle (position, size), pen, brush)
		{
		}

		public Ellipse (Point position, double diameter)
			: this (position, new Size (diameter))
		{
		}

		public Ellipse (double diameter)
			: this (Point.Zero, new Size (diameter))
		{
		}

		public void Draw (ICanvas surface)
		{
			surface.DrawEllipse (frame, pen, brush);
		}
	}
}

