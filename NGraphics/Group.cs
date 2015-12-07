using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public class Group : Element
	{
		public readonly List<Element> Children = new List<Element> ();

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

		public override EdgeSamples[] GetEdgeSamples (double tolerance, int minSamples, int maxSamples)
		{
			var edges = new List<EdgeSamples> ();
			foreach (var c in Children.OfType<IEdgeSampleable> ()) {
				edges.AddRange (c.GetEdgeSamples (tolerance, minSamples, maxSamples));
			}
			for (int i = 0; i < edges.Count; i++) {
				for (int j = 0; j < edges [i].Points.Length; j++) {
					var p = Transform.TransformPoint (edges [i].Points[j]);
					edges [i].Points[j] = p;
				}
			}
			return edges.ToArray ();
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
