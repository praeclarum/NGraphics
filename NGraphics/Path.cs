using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	public abstract class PathOp
	{
		public abstract Point GetContinueCurveControlPoint ();
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

		public override Point GetContinueCurveControlPoint ()
		{
			return Point;
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
		public override Point GetContinueCurveControlPoint ()
		{
			return Point;
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
		public override Point GetContinueCurveControlPoint ()
		{
			return Control2.ReflectedAround (Point);
		}
	}
	public class ClosePath : PathOp
	{
		public override Point GetContinueCurveControlPoint ()
		{
			throw new NotSupportedException ();
		}
	}

	public class Path : Element
	{
		public readonly List<PathOp> Operations = new List<PathOp> ();

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
			Operations.Add (op);
		}

		public void MoveTo (Point point)
		{
			Add (new MoveTo (point));
		}
		public void MoveTo (double x, double y)
		{
			Add (new MoveTo (x, y));
		}

		public void LineTo (Point point)
		{
			Add (new LineTo (point));
		}
		public void LineTo (double x, double y)
		{
			Add (new LineTo (x, y));
		}

		public void CurveTo (Point control1, Point control2, Point point)
		{
			Add (new CurveTo (control1, control2, point));
		}

		public void ContinueCurveTo (Point control2, Point point)
		{
			if (Operations.Count == 0) {
				throw new InvalidOperationException ("Cannot continue a curve until the path has begun with another operation.");
			}
			var prev = Operations [Operations.Count - 1];
			var control1 = prev.GetContinueCurveControlPoint ();
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

