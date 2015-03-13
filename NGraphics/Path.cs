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
	public class ArcTo : PathOp
	{
		public Size Radius;
		public bool LargeArc;
		public bool SweepClockwise;
		public Point Point;
		public ArcTo (Size radius, bool largeArc, bool sweepClockwise, Point point)
		{
			Radius = radius;
			LargeArc = largeArc;
			SweepClockwise = sweepClockwise;
			Point = point;
		}
		public override Point GetContinueCurveControlPoint ()
		{
			return Point;
		}
		public void GetCircles (Point prevPoint, out Point circle1Center, out Point circle2Center)
		{
			//Following explanation at http://mathforum.org/library/drmath/view/53027.html'
			if (Radius.Width == 0)
				throw new Exception ("radius x of zero");
			if (Radius.Height == 0)
				throw new Exception ("radius y of zero");
			var p1 = prevPoint;
			var p2 = Point;
			if (p1 == p2)
				throw new Exception ("coincident points gives infinite number of Circles");
			// delta x, delta y between points
			var dp = p2 - p1;
			// dist between points
			var q = dp.Distance;
			if (q > 2.0*Radius.Diagonal)
				throw new Exception ("separation of points > diameter");
			// halfway point
			var p3 = (p1 + p2) / 2;
			// distance along the mirror line
			var xd = Math.Sqrt (Radius.Width*Radius.Width - (q/2)*(q/2));
			var yd = Math.Sqrt (Radius.Height*Radius.Height - (q/2)*(q/2));

			circle1Center = new Point (p3.X - yd * dp.Y / q, p3.Y + xd * dp.X / q);
			circle2Center = new Point (p3.X + yd * dp.Y / q, p3.Y - xd * dp.X / q);
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

		public void ArcTo (Size radius, bool largeArc, bool sweepClockwise, Point point)
		{
			Add (new ArcTo (radius, largeArc, sweepClockwise, point));
		}
		public void CurveTo (Point control1, Point control2, Point point)
		{
			Add (new CurveTo (control1, control2, point));
		}
		public void CurveTo (double c1x, double c1y, double c2x, double c2y, double x, double y)
		{
			CurveTo (new Point (c1x, c1y), new Point (c2x, c2y), new Point (x, y));
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
		public void ContinueCurveTo (double c2x, double c2y, double x, double y)
		{
			ContinueCurveTo (new Point (c2x, c2y), new Point (x, y));
		}

		public void Close ()
		{
			Add (new ClosePath ());
		}

		public bool Contains (Point point)
		{
			var verts = new List<Point> ();
			foreach (var o in Operations) {
				var mo = o as MoveTo;
				if (mo != null) {
					verts.Add (mo.Point);
					continue;
				}
				var lt = o as LineTo;
				if (lt != null) {
					verts.Add (lt.Point);
					continue;
				}
				var cp = o as ClosePath;
				if (cp != null) {
					continue;
				}
				throw new NotSupportedException ("Contains does not support " + o);
			}
			int i, j;
			var c = false;
			var nverts = verts.Count;
			var testx = point.X;
			var testy = point.Y;
			for (i = 0, j = nverts-1; i < nverts; j = i++) {
				if ( ((verts[i].Y>testy) != (verts[j].Y>testy)) &&
					(testx < (verts[j].X-verts[i].X) * (testy-verts[i].Y) / (verts[j].Y-verts[i].Y) + verts[i].X) )
					c = !c;
			}
			return c;
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Path ([{0}])", Operations.Count);
		}
	}
}

