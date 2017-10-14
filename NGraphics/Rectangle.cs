using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public class Rectangle : Element
	{
		Rect frame;
		Size corner;

		public Rect Frame { get { return frame; } }
		public Size Corner { get { return corner; } }

		public Rectangle (Rect frame, Size corner, Pen pen = null, Brush brush = null)
			: base (pen, brush)
		{
			this.frame = frame;
			this.corner = corner;
		}

		public Rectangle (Rect frame, Pen pen = null, Brush brush = null)
			: base (pen, brush)
		{
			this.frame = frame;
			this.corner = Size.Zero;
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

		protected override void AcceptVisitor (IElementVisitor visitor)
		{
			visitor.Visit (this);
			visitor.EndVisit (this);
		}

		protected override void DrawElement (ICanvas canvas)
		{
			canvas.DrawRectangle (frame, corner, Pen, Brush);
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Rectangle ({0})", frame);
		}

		protected override Element CreateUninitializedClone ()
		{
			return new Rectangle (frame);
		}

		public override Element TransformGeometry (Transform transform)
		{
			var p = new Path ();
			base.SetCloneData (p);
			p.Transform = Transform.Identity;
			p.MoveTo (frame.TopLeft);
			p.LineTo (frame.BottomLeft);
			p.LineTo (frame.BottomRight);
			p.LineTo (frame.TopRight);
			p.Close ();
			var tp = p.TransformGeometry (transform * Transform);
			return tp;
		}

		public override bool Contains (Point localPoint)
		{
			return frame.Contains (localPoint);
		}

		#region ISampleable implementation

		public override EdgeSamples[] GetEdgeSamples (double tolerance, int minSamples, int maxSamples)
		{
			var r = new List<Point> ();
			r.AddRange (SampleLine (Frame.TopLeft, Frame.BottomLeft, false, tolerance, minSamples, maxSamples));
			r.AddRange (SampleLine (Frame.BottomLeft, Frame.BottomRight, false, tolerance, minSamples, maxSamples));
			r.AddRange (SampleLine (Frame.BottomRight, Frame.TopRight, false, tolerance, minSamples, maxSamples));
			r.AddRange (SampleLine (Frame.TopRight, Frame.TopLeft, true, tolerance, minSamples, maxSamples));
			for (int i = 0; i < r.Count; i++) {
				var p = Transform.TransformPoint (r [i]);
				r [i] = p;
			}
			return new[] { new EdgeSamples { Points = r.ToArray () } };
		}

		[System.Runtime.Serialization.IgnoreDataMember]
		public override Rect SampleableBox {
			get {
				return Transform.TransformRect (Frame);
			}
		}

		#endregion
	}
}

