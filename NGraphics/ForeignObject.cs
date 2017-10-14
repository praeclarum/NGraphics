using System;

namespace NGraphics
{
	public class ForeignObject : Element
	{
		protected Point pos;
		protected Size size;

		public ForeignObject (Point pos, Size size) : base(null, null)
		{
			this.pos = pos;
			this.size = size;
		}

		protected override void AcceptVisitor (IElementVisitor visitor)
		{
			visitor.Visit (this);
			visitor.EndVisit (this);
		}

		protected override Element CreateUninitializedClone ()
		{
			return new ForeignObject (pos, size);
		}

		public override Element TransformGeometry (Transform transform)
		{
			var frame = new Rect (pos, size);
			var tframe = transform.TransformRect (frame);
			return new ForeignObject (tframe.TopLeft, tframe.Size);
		}

		public override bool Contains (Point localPoint)
		{
			var frame = new Rect (pos, size);
			return frame.Contains (localPoint);
		}

		protected override void DrawElement (ICanvas canvas)
		{
			throw new NotSupportedException();
		}

		public override EdgeSamples[] GetEdgeSamples (double tolerance, int minSamples, int maxSamples)
		{
			throw new NotSupportedException ();
		}

		public override Rect SampleableBox {
			get {
				return new Rect (pos, size);
			}
		}
	}
}

