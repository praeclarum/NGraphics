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

		protected override Element CreateUninitializedClone ()
		{
			return new Group ();
		}

		protected override void SetCloneData (Element clone)
		{
			base.SetCloneData (clone);
			((Group)clone).Children.AddRange (Children.Select (x => x.Clone ()));
		}

		public override Element TransformGeometry (Transform transform)
		{
			var clone = (Group)Clone ();
			clone.Transform = Transform.Identity;
			var tt = transform * Transform;
			clone.Children.Clear ();
			clone.Children.AddRange (Children.Select (x => x.TransformGeometry (tt)));
			return clone;
		}

		public override bool Contains (Point point)
		{
			throw new NotImplementedException ();
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
