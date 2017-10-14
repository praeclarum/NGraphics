using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public class Group : Element
	{
		public readonly List<Element> Children = new List<Element> ();
		private double _opacity = 1.0;
		private double _resetOpacityMultiplier = 1.0;

		public Group ()
			: base (null, null)
		{			
		}

		protected override void AcceptVisitor (IElementVisitor visitor)
		{
			visitor.Visit (this);
			foreach (var c in Children) {
				c.Accept (visitor);
			}
			visitor.EndVisit (this);
		}

		public double Opacity {
			get { return _opacity; }
			set {

				if (value == _opacity)
					return;

				_resetOpacityMultiplier = _opacity/value;
				_opacity = value;
			}
		}

		public override Pen Pen {
			get {
				return base.Pen;
			}
			set {
				base.Pen = value;
				foreach (var c in Children) {
					c.Pen = value;
				}
			}
		}

		public override Brush Brush {
			get {
				return base.Brush;
			}
			set {
				base.Brush = value;
				foreach (var c in Children) {
					c.Brush = value;
				}
			}
		}

		protected override void DrawElement (ICanvas canvas)
		{
			bool setOpacity = Opacity != 1.0 && _resetOpacityMultiplier != 1.0;
			var setOpacityVisitor = setOpacity ? new OpacityVisitor{ Opacity = Opacity } : null;
			var resetOpacityVisitor = setOpacity ? new OpacityVisitor{ Opacity = _resetOpacityMultiplier } : null;

			foreach (var c in Children) {

				if (setOpacityVisitor != null)
					setOpacityVisitor.VisitElement (c);
				
				c.Draw (canvas);

				if (resetOpacityVisitor != null)
					resetOpacityVisitor.VisitElement (c);
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

		public override bool Contains (Point localPoint)
		{
			foreach (var c in Children) {
				if (c.HitTest (localPoint))
					return true;
			}
			return false;
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

	class OpacityVisitor : BaseElementVisitor
	{
		public double Opacity;

		public override void VisitElement (Element element)
		{
			if (element.Brush != null) {				
				// Modify alpha
				if (element.Brush is SolidBrush) 
					(element.Brush as SolidBrush).Color = (element.Brush as SolidBrush).Color.WithAlpha(
						(element.Brush as SolidBrush).Color.Alpha * Opacity);
					
				else if (element.Brush is GradientBrush) 
					foreach (var stop in (element.Brush as GradientBrush).Stops)
						stop.Color = stop.Color.WithAlpha(stop.Color.Alpha * Opacity);				
			}

			if (element.Pen != null)				
				// Modify alpha
				element.Pen.Color = element.Pen.Color.WithAlpha(element.Pen.Color.Alpha * Opacity);
		}
	}
}
