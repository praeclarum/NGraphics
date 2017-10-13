using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	public abstract class Element : IDrawable, IEdgeSampleable
	{
		public string Id { get; set; }
		public Transform Transform { get; set; }
		public virtual Pen Pen { get; set; }
		public virtual Brush Brush { get; set; }

		protected Element (Pen pen, Brush brush)
		{
			Id = "";
			Pen = pen;
			Brush = brush;
			Transform = NGraphics.Transform.Identity;
		}

		protected abstract void DrawElement (ICanvas canvas);

		protected virtual void SetCloneData (Element clone)
		{
			clone.Id = Id;
			clone.Transform = Transform;
			clone.Pen = Pen;
			clone.Brush = Brush;
		}
		protected abstract Element CreateUninitializedClone ();
		public Element Clone ()
		{
			var r = CreateUninitializedClone ();
			SetCloneData (r);
			return r;
		}

		public abstract Element TransformGeometry (Transform transform);

		public abstract bool Contains (Point localPoint);

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

		public static Point[] SampleLine (Point begin, Point end, bool includeEnd, double tolerance, int minSamples, int maxSamples)
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

		public abstract EdgeSamples[] GetEdgeSamples (double tolerance, int minSamples, int maxSamples);

		[System.Runtime.Serialization.IgnoreDataMember]
		public abstract Rect SampleableBox {
			get;
		}

		#endregion

		public bool HitTest (Point worldPoint)
		{
			var localPoint = Transform.GetInverse ().TransformPoint (worldPoint);
			return Contains (localPoint);
		}

		protected abstract void AcceptVisitor (IElementVisitor visitor);

		public void Accept (IElementVisitor visitor)
		{
			AcceptVisitor (visitor);
		}
	}

	public interface IElementVisitor
	{
		void Visit (Rectangle rectangle);
		void Visit (Ellipse ellipse);
		void Visit (Group group);
		void Visit (Path path);
		void Visit (Text text);
		void Visit (ForeignObject foreignObject);
		void EndVisit (Rectangle rectangle);
		void EndVisit (Ellipse ellipse);
		void EndVisit (Group group);
		void EndVisit (Path path);
		void EndVisit (Text text);
		void EndVisit (ForeignObject foreignObject);
	}
	public class BaseElementVisitor : IElementVisitor
	{
		public virtual void VisitElement (Element element)
		{
		}
		public virtual void EndVisitElement (Element element)
		{
		}
		public virtual void Visit (Rectangle rectangle)
		{
			VisitElement (rectangle);
		}
		public virtual void Visit (Ellipse ellipse)
		{
			VisitElement (ellipse);
		}
		public virtual void Visit (Group group)
		{
			VisitElement (group);
		}
		public virtual void Visit (Path path)
		{
			VisitElement (path);
		}
		public virtual void Visit (Text text)
		{
			VisitElement (text);
		}
		public virtual void Visit (ForeignObject foreignObject)
		{
			VisitElement (foreignObject);
		}
		public virtual void EndVisit (Rectangle rectangle)
		{
			EndVisitElement (rectangle);
		}
		public virtual void EndVisit (Ellipse ellipse)
		{
			EndVisitElement (ellipse);
		}
		public virtual void EndVisit (Group group)
		{
			EndVisitElement (group);
		}
		public virtual void EndVisit (Path path)
		{
			EndVisitElement (path);
		}
		public virtual void EndVisit (Text text)
		{
			EndVisitElement (text);
		}
		public virtual void EndVisit (ForeignObject foreignObject)
		{
			EndVisitElement (foreignObject);
		}
	}
}
