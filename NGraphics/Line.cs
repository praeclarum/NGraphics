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
	}
}

