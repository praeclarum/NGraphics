using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public abstract class PathOp
	{
		public abstract Point GetContinueCurveControlPoint ();
		public abstract Point GetEndPoint (Point startPoint);
		public Point EndPoint { get { return GetEndPoint (Point.Zero); } }
		public abstract EdgeSamples[] GetEdgeSamples (Point startPoint, Point prevPoint, double tolerance, int minSamples, int maxSamples);
		public abstract PathOp Clone ();
		public abstract void TransformGeometry (Point prevPoint, Transform transform);
		public abstract double DistanceTo (Point startPoint, Point prevPoint, Point point);
		protected abstract void AcceptVisitor (IPathOpVisitor visitor);
		public void Accept (IPathOpVisitor visitor) {
			AcceptVisitor (visitor);
		}
	}
	public class MoveTo : PathOp
	{
		public Point Point;
		public MoveTo (Point point)
		{
			Point = point;
		}
		protected override void AcceptVisitor (IPathOpVisitor visitor)
		{
			visitor.Visit (this);
		}
		public MoveTo (double x, double y)
			: this (new Point (x, y))
		{
		}
		public override PathOp Clone ()
		{
			return new MoveTo (Point);
		}


		public override Point GetContinueCurveControlPoint ()
		{
			return Point;
		}

		public override Point GetEndPoint (Point startPoint) { return Point; }

		public override EdgeSamples[] GetEdgeSamples (Point startPoint, Point prevPoint, double tolerance, int minSamples, int maxSamples)
		{
			return new EdgeSamples[0];
		}

		public override void TransformGeometry (Point prevPoint, Transform transform)
		{
			Point = transform.TransformPoint (Point);
		}

		public override double DistanceTo (Point startPoint, Point prevPoint, Point point)
		{
			return point.DistanceTo (Point);
		}

		public override string ToString ()
		{
			return string.Format ("MoveTo ({0})", Point);
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
		protected override void AcceptVisitor (IPathOpVisitor visitor)
		{
			visitor.Visit (this);
		}
		public override PathOp Clone ()
		{
			return new LineTo (Point);
		}
		public override Point GetContinueCurveControlPoint ()
		{
			return Point;
		}

		public override Point GetEndPoint (Point startPoint) { return Point; }

		public override EdgeSamples[] GetEdgeSamples (Point startPoint, Point prevPoint, double tolerance, int minSamples, int maxSamples)
		{
			var ps = Element.SampleLine (prevPoint, Point, true, tolerance, minSamples, maxSamples);
			return new [] { new EdgeSamples { Points = ps } };
		}

		public override void TransformGeometry (Point prevPoint, Transform transform)
		{
			Point = transform.TransformPoint (Point);
		}

		public override double DistanceTo (Point startPoint, Point prevPoint, Point point)
		{
			return point.DistanceToLineSegment (prevPoint, Point);
		}

		public override string ToString ()
		{
			return string.Format ("LineTo ({0})", Point);
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
		protected override void AcceptVisitor (IPathOpVisitor visitor)
		{
			visitor.Visit (this);
		}
		public override PathOp Clone ()
		{
			return new ArcTo (Radius, LargeArc, SweepClockwise, Point);
		}
		public override Point GetContinueCurveControlPoint ()
		{
			return Point;
		}

		public override Point GetEndPoint (Point startPoint) { return Point; }

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

		public override EdgeSamples[] GetEdgeSamples (Point startPoint, Point prevPoint, double tolerance, int minSamples, int maxSamples)
		{
			throw new NotSupportedException ();
		}

		public override void TransformGeometry (Point prevPoint, Transform transform)
		{
			throw new NotSupportedException ();
		}

		public override double DistanceTo (Point startPoint, Point prevPoint, Point point)
		{
			throw new NotSupportedException ();
		}

		public override string ToString ()
		{
			return string.Format ("ArcTo ({0})", Point);
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
		protected override void AcceptVisitor (IPathOpVisitor visitor)
		{
			visitor.Visit (this);
		}
		public override PathOp Clone ()
		{
			return new CurveTo (Control1, Control2, Point);
		}

		public override Point GetContinueCurveControlPoint ()
		{
			return Control2.ReflectedAround (Point);
		}
		public override Point GetEndPoint (Point startPoint) { return Point; }
		public override EdgeSamples[] GetEdgeSamples (Point startPoint, Point prevPoint, double tolerance, int minSamples, int maxSamples)
		{
			var n = (3*prevPoint.DistanceTo (Point)) / tolerance;
			if (n < minSamples)
				n = minSamples;
			if (n > maxSamples)
				n = maxSamples;

			var r = new List<Point> ();

			var dt = 1.0 / (n - 1);

			for (var i = 0; i < n; i++) {
				var t = i * dt;
				var p = GetPoint (prevPoint, t);
				r.Add (p);
			}

			return new[]{ new EdgeSamples { Points = r.ToArray () } };
		}
		public override void TransformGeometry (Point prevPoint, Transform transform)
		{
			Point = transform.TransformPoint (Point);
			Control1 = transform.TransformPoint (Control1);
			Control2 = transform.TransformPoint (Control2);
		}

		public Point GetPoint (Point prevPoint, double t)
		{
			var u = 1 - t;
			return
				u * u * u * prevPoint +
				3 * u * u * t * Control1 +
				3 * u * t * t * Control2 +
				t * t * t * Point;
		}

		public override double DistanceTo (Point startPoint, Point prevPoint, Point point)
		{
			var edges = GetEdgeSamples (startPoint, prevPoint, 1, 16, 16);
			return edges [0].DistanceTo (point);
		}

		public override string ToString ()
		{
			return string.Format ("CurveTo ({0})", Point);
		}
	}
	public class ClosePath : PathOp
	{
		protected override void AcceptVisitor (IPathOpVisitor visitor)
		{
			visitor.Visit (this);
		}
		public override PathOp Clone ()
		{
			return new ClosePath ();
		}
		public override Point GetContinueCurveControlPoint ()
		{
			throw new NotSupportedException ();
		}
		public override Point GetEndPoint (Point startPoint) { return startPoint; }
		public override EdgeSamples[] GetEdgeSamples (Point startPoint, Point prevPoint, double tolerance, int minSamples, int maxSamples)
		{
			if (prevPoint.DistanceTo (startPoint) < tolerance) {
				return new EdgeSamples[0];
			}
			var ps = Element.SampleLine (prevPoint, startPoint, true, tolerance, minSamples, maxSamples);
			return new [] { new EdgeSamples { Points = ps } };
		}
		public override void TransformGeometry (Point prevPoint, Transform transform)
		{
		}
		public override double DistanceTo (Point startPoint, Point prevPoint, Point point)
		{
			return point.DistanceToLineSegment (prevPoint, startPoint);
		}
		public override string ToString ()
		{
			return string.Format ("Close ()");
		}
	}

	public interface IPathOpVisitor
	{
		void Visit (MoveTo moveTo);
		void Visit (LineTo lineTo);
		void Visit (CurveTo curveTo);
		void Visit (ArcTo arcTo);
		void Visit (ClosePath closePath);
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

		protected override void AcceptVisitor (IElementVisitor visitor)
		{
			visitor.Visit (this);
			visitor.EndVisit (this);
		}

		public void AcceptPathOpVisitor (IPathOpVisitor visitor)
		{
			foreach (var op in Operations) {
				op.Accept (visitor);
			}
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

		public override bool Contains (Point localPoint)
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
			var testx = localPoint.X;
			var testy = localPoint.Y;
			for (i = 0, j = nverts-1; i < nverts; j = i++) {
				if ( ((verts[i].Y>testy) != (verts[j].Y>testy)) &&
					(testx < (verts[j].X-verts[i].X) * (testy-verts[i].Y) / (verts[j].Y-verts[i].Y) + verts[i].X) )
					c = !c;
			}
			return c;
		}

		public double DistanceToLocal (Point localPoint)
		{
			var startPoint = Point.Zero;
			var prevPoint = startPoint;

			var minD = double.MaxValue;

			foreach (var op in Operations) {
				if (op is MoveTo) {
					startPoint = op.EndPoint;
				}

				var d = op.DistanceTo (startPoint, prevPoint, localPoint);

				minD = Math.Min (d, minD);

				prevPoint = op.GetEndPoint (startPoint);
			}

			return minD;
		}

		public double DistanceTo (Point worldPoint)
		{
			return DistanceToLocal (Transform.GetInverse ().TransformPoint (worldPoint));
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Path ([{0}])", Operations.Count);
		}

		protected override Element CreateUninitializedClone ()
		{
			return new Path ();
		}

		protected override void SetCloneData (Element clone)
		{
			base.SetCloneData (clone);
			((Path)clone).Operations.AddRange (Operations.Select (x => x.Clone ()));
		}

		public override Element TransformGeometry (Transform transform)
		{
			var clone = (Path)Clone ();

			var tt = transform * Transform;

			clone.Transform = Transform.Identity;

			var startPoint = Point.Zero;
			var prevPoint = startPoint;

			foreach (var op in clone.Operations) {
				if (op is MoveTo) {
					startPoint = transform.TransformPoint (op.EndPoint);
				}
				op.TransformGeometry (prevPoint, tt);
				prevPoint = op.GetEndPoint (startPoint);
			}

			return clone;
		}

		public override Rect SampleableBox {
			get {
				var edges = GetEdgeSamples (1, 2, 8);
				var bbb = new BoundingBoxBuilder ();
				foreach (var e in edges) {
					bbb.Add (e.Points);
				}
				return bbb.BoundingBox;
			}
		}

		public override EdgeSamples[] GetEdgeSamples (double tolerance, int minSamples, int maxSamples)
		{
			var edges = new List<EdgeSamples> ();

			var startPoint = Point.Zero;
			var prevPoint = startPoint;

			foreach (var op in Operations) {
				if (op is MoveTo) {
					startPoint = op.EndPoint;
				}
				edges.AddRange (op.GetEdgeSamples (startPoint, prevPoint, tolerance, minSamples, maxSamples));
				prevPoint = op.GetEndPoint (startPoint);
			}

			for (int i = 0; i < edges.Count; i++) {
				var e = edges [i];
				for (int j = 0; j < e.Points.Length; j++) {
					var p = Transform.TransformPoint (e.Points [j]);
					e.Points [j] = p;
				}
			}

			return edges.ToArray ();
		}

	}
}

