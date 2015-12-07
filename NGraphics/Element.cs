using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	public abstract class Element : IDrawable, ISampleable
	{
		public string Id { get; set; }
		public Transform Transform { get; set; }
		public Pen Pen { get; set; }
		public Brush Brush { get; set; }

		protected Element (Pen pen, Brush brush)
		{
			Id = Guid.NewGuid ().ToString ();
			Pen = pen;
			Brush = brush;
			Transform = NGraphics.Transform.Identity;
		}

		protected abstract void DrawElement (ICanvas canvas);

		#region IDrawable implementation

		public void Draw (ICanvas canvas)
		{
			var t = Transform;
			var pushedState = false;
			try {
				if (t != NGraphics.Transform.Identity) {
					canvas.SaveState ();
					pushedState = true;
					canvas.Transform (t);
				}
				DrawElement (canvas);
			} finally {
				if (pushedState) {
					canvas.RestoreState ();
				}
			}
		}

		#endregion

		#region ISampleable implementation

		protected Point[] SampleLine (Point begin, Point end, bool includeEnd, double tolerance, int minSamples, int maxSamples)
		{
			var r = new List<Point> ();
			var d = end - begin;
			var dist = d.Distance;
			var n = (int)Math.Round (dist / tolerance);
			if (n < minSamples)
				n = minSamples;
			if (n > maxSamples)
				n = maxSamples;
			var dt = 1.0 / (n - 1);
			var endN = includeEnd ? n : n - 1;
			for (var i = 0; i < endN; i++) {
				var t = i * dt;
				var p = begin + d * t;
				r.Add (p);
			}
			return r.ToArray ();
		}

		public abstract Point[] GetSamples (double tolerance, int minSamples, int maxSamples);

		[System.Runtime.Serialization.IgnoreDataMember]
		public abstract Rect SampleableBox {
			get;
		}

		#endregion
	}


}
