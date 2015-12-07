using System;
using System.Globalization;

namespace NGraphics
{
	public class Line : Element
	{
		protected Point start;
		protected Point end;

		public Line (Point start, Point end, Pen pen)
			: base (pen, null)
		{
			this.start = start;
			this.end = end;
		}

		protected override void DrawElement (ICanvas canvas)
		{
			canvas.DrawLine(start, end, Pen);
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Line ({0}-{1})", start, end);
		}

		#region implemented abstract members of Element

		public override EdgeSamples[] GetEdgeSamples (double tolerance, int minSamples, int maxSamples)
		{
			var ps = SampleLine (start, end, true, tolerance, minSamples, maxSamples);
			return new[] { new EdgeSamples { Points = ps } };
		}

		public override Rect SampleableBox {
			get {
				var bb = new Rect (start, Size.Zero);
				return bb.Union (end);
			}
		}

		#endregion
	}
}

