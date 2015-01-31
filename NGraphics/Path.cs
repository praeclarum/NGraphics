using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	public abstract class PathOp
	{
	}
	public class MoveTo : PathOp
	{
		public Point Point;
		public MoveTo (Point point)
		{
			Point = point;
		}
		public MoveTo (double x, double y)
			: this (new Point (x, y))
		{
		}
	}
	public class LineTo : PathOp
	{
		public Point Point;
		public LineTo (Point point)
		{
			Point = point;
		}
		public LineTo (double x, double y)
			: this (new Point (x, y))
		{
		}
	}
	public class CurveTo : PathOp
	{
		public Point Control1;
		public Point Control2;
		public Point Point;
		public CurveTo (Point control1, Point control2, Point point)
		{
			Control1 = control1;
			Control2 = control2;
			Point = point;
		}
	}
	public class ClosePath : PathOp
	{
	}

	public class Path : Element
	{
		public readonly List<PathOp> Operations = new List<PathOp> ();

		public bool IsClosed {
			get {
				return Operations.Count > 0 && Operations [Operations.Count - 1] is ClosePath;
			}
		}

		public Path (IEnumerable<PathOp> operations, Pen pen = null, Brush brush = null)
			: base (pen, brush)
		{
			Operations.AddRange (operations);
		}
		public Path (Pen pen = null, Brush brush = null)
			: base (pen, brush)
		{
		}

		protected override void DrawElement (ICanvas canvas)
		{
			canvas.DrawPath (Operations, Pen, Brush);
		}

		void Add (PathOp op)
		{
			if (IsClosed)
				return;
			Operations.Add (op);
		}

		public void MoveTo (Point point)
		{
			Add (new MoveTo (point));
		}

		public void LineTo (Point point)
		{
			Add (new LineTo (point));
		}

		public void CurveTo (Point control1, Point control2, Point point)
		{
			Add (new CurveTo (control1, control2, point));
		}

		public void Close ()
		{
			Add (new ClosePath ());
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Path ([{0}])", Operations.Count);
		}
	}
}

