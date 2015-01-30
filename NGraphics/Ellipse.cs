using System;
using System.Globalization;

namespace NGraphics
{
	public class Ellipse : IDrawable
	{
		Rect frame;
		Pen pen;
		Brush brush;

		public Ellipse (Rect frame, Pen pen = null, Brush brush = null)
		{
			this.frame = frame;
			this.pen = pen;
			this.brush = brush;
		}

		public Ellipse (Point position, Size size, Pen pen = null, Brush brush = null)
			: this (new Rect (position, size), pen, brush)
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

		public void Draw (ICanvas canvas)
		{
			canvas.DrawEllipse (frame, pen, brush);
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Ellipse ({0})", frame);
		}
	}
}

