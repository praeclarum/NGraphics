using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public class Group : Element, IEdgeSampleable
	{
		public readonly List<IDrawable> Children = new List<IDrawable> ();

		public Group ()
			: base (null, null)
		{			
		}

		protected override void DrawElement (ICanvas canvas)
		{
			foreach (var c in Children) {
				c.Draw (canvas);
			}
		}

		public override string ToString ()
		{
			return "[" + string.Join (", ", Children.Select (x => x.ToString ())) + "]";
		}

		#region ISampleable implementation

		public override Point[] GetEdgeSamples (double tolerance, int minSamples, int maxSamples)
		{
			var points = new List<Point> ();
			foreach (var c in Children.OfType<IEdgeSampleable> ()) {
				points.AddRange (c.GetEdgeSamples (tolerance, minSamples, maxSamples));
			}
			for (int i = 0; i < points.Count; i++) {
				var p = Transform.TransformPoint (points [i]);
				points [i] = p;
			}
			return points.ToArray ();
		}

		[System.Runtime.Serialization.IgnoreDataMember]
		public override Rect SampleableBox {
			get {
				var r = Rect.Union (Children.OfType<IEdgeSampleable> ().Select (x => x.SampleableBox));
				return Transform.TransformRect (r);
			}
		}

		#endregion
	}
	
}
