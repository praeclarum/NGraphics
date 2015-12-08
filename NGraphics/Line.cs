using System;
using System.Globalization;

namespace NGraphics
{
	public class Line : Element
	{
		protected Point start;
		protected Point end;

		public Line (Point start, Point end, Pen pen = null)
			: base (pen, null)
		{
			this.start = start;
			this.end = end;
		}

		protected override Element CreateUninitializedClone ()
		{
			return new Line (start, end);
		}

		protected override void SetCloneData (Element clone)
		{
			base.SetCloneData (clone);
		}

		public override Element TransformGeometry (Transform transform)
		{			
			var clone = (Line)Clone ();
			clone.start = transform.TransformPoint (start);
			clone.end = transform.TransformPoint (end);
			return clone;
		}

		public override bool Contains (Point point)
		{
			return false;
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
			for (int i = 0; i < ps.Length; i++) {
				var p = Transform.TransformPoint (ps [i]);
				ps [i] = p;
			}
			return new[] { new EdgeSamples { Points = ps } };
		}

		public override Rect SampleableBox {
			get {
				var bb = new Rect (start, Size.Zero);
				return Transform.TransformRect (bb.Union (end));
			}
		}

		#endregion
	}
}

