using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	public class Ellipse : Element
	{
		Rect frame;

		public Ellipse (Rect frame, Pen pen = null, Brush brush = null)
			: base (pen, brush)
		{
			this.frame = frame;
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

		protected override void DrawElement (ICanvas canvas)
		{
			canvas.DrawEllipse (frame, Pen, Brush);
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Ellipse ({0})", frame);
		}

		public override Rect SampleableBox {
			get {
				return Transform.TransformRect (frame);
			}
		}

		public override Point[] GetEdgeSamples (double tolerance, int minSamples, int maxSamples)
		{
			//https://en.wikipedia.org/wiki/Ellipse#Circumference
			var a = frame.Width / 2;
			var b = frame.Height / 2;
			var center = frame.Center;
			var circumference = Math.PI * (3*(a+b) - Math.Sqrt (10*a*b + 3*(a*a + b*b)));
			var n = (int)(Math.Round (circumference / tolerance));
			if (n < minSamples)
				n = minSamples;
			if (n > maxSamples)
				n = maxSamples;
			var da = 2 * Math.PI / n;
			var r = new List<Point> ();
			for (var i = 0; i < n; i++) {
				var x = center.X + a * Math.Cos (i * da);
				var y = center.Y + b * Math.Sin (i * da);
				r.Add (Transform.TransformPoint (new Point (x, y)));
			}
			return r.ToArray ();
		}
	}
}

