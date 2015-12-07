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

