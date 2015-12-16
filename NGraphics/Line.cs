using System;
using System.Globalization;

namespace NGraphics
{
	public class Line : Element
	{
		public Point Start;
		public Point End;

		public Line (Point start, Point end, Pen pen = null)
			: base (pen, null)
		{
			this.Start = start;
			this.End = end;
		}

		protected override void AcceptVisitor (IElementVisitor visitor)
		{
			visitor.Visit (this);
			visitor.EndVisit (this);
		}

		protected override Element CreateUninitializedClone ()
		{
			return new Line (Start, End);
		}

		protected override void SetCloneData (Element clone)
		{
			base.SetCloneData (clone);
		}

		public override Element TransformGeometry (Transform transform)
		{			
			var clone = (Line)Clone ();
			clone.Start = transform.TransformPoint (Start);
			clone.End = transform.TransformPoint (End);
			return clone;
		}

		public override bool Contains (Point point)
		{
			return false;
		}

		protected override void DrawElement (ICanvas canvas)
		{
			canvas.DrawLine (Start, End, Pen);
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Line ({0}, {1})", Start, End);
		}

		#region implemented abstract members of Element

		public override EdgeSamples[] GetEdgeSamples (double tolerance, int minSamples, int maxSamples)
		{
			var ps = SampleLine (Start, End, true, tolerance, minSamples, maxSamples);
			for (int i = 0; i < ps.Length; i++) {
				var p = Transform.TransformPoint (ps [i]);
				ps [i] = p;
			}
			return new[] { new EdgeSamples { Points = ps } };
		}

		public override Rect SampleableBox {
			get {
				var bb = new Rect (Start, Size.Zero);
				return Transform.TransformRect (bb.Union (End));
			}
		}

		#endregion
	}
}

