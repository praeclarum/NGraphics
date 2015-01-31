using System;
using System.Globalization;

namespace NGraphics
{
	public class Rectangle : Element
	{
		Rect frame;

		public Rectangle (Rect frame, Pen pen = null, Brush brush = null)
			: base (pen, brush)
		{
			this.frame = frame;
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

		protected override void DrawElement (ICanvas canvas)
		{
			canvas.DrawRectangle (frame, Pen, Brush);
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Rectangle ({0})", frame);
		}
	}
}

