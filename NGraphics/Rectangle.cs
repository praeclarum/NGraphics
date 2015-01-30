using System;
using System.Globalization;

namespace NGraphics
{
	public class Rectangle : IDrawable
	{
		Rect frame;
		Pen pen;
		Brush brush;

		public Rectangle (Rect frame, Pen pen = null, Brush brush = null)
		{
			this.frame = frame;
			this.pen = pen;
			this.brush = brush;
		}

		public Rectangle (Point position, Size size, Pen pen = null, Brush brush = null)
			: this (new Rect (position, size), pen, brush)
		{
		}

		public Rectangle (Point position, double size)
			: this (position, new Size (size))
		{
		}

		public Rectangle (double size)
			: this (Point.Zero, new Size (size))
		{
		}

		public void Draw (ICanvas canvas)
		{
			canvas.DrawRectangle (frame, pen, brush);
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Rectangle ({0})", frame);
		}
	}
}

