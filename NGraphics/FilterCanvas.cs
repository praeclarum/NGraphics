using System;

namespace NGraphics
{
	public class FilterCanvas : ICanvas
	{
		public readonly ICanvas NextCanvas;

		public FilterCanvas (ICanvas nextCanvas)
		{
			if (nextCanvas == null)
				throw new ArgumentNullException ("nextCanvas");
			NextCanvas = nextCanvas;
		}

		#region ICanvas implementation

		public virtual void SaveState ()
		{
			NextCanvas.SaveState ();
		}

		public virtual void Transform (Transform transform)
		{
			NextCanvas.Transform (transform);
		}

		public virtual void RestoreState ()
		{
			NextCanvas.RestoreState ();
		}

		public virtual void DrawText (string text, Rect frame, Font font, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null)
		{
			NextCanvas.DrawText (text, frame, font, alignment, pen, brush);
		}

		public virtual void DrawPath (System.Collections.Generic.IEnumerable<PathOp> ops, Pen pen = null, Brush brush = null)
		{
			NextCanvas.DrawPath (ops, pen, brush);
		}

		public virtual void DrawRectangle (Rect frame, Pen pen = null, Brush brush = null)
		{
			NextCanvas.DrawRectangle (frame, pen, brush);
		}

		public virtual void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null)
		{
			NextCanvas.DrawEllipse (frame, pen, brush);
		}

		public virtual void DrawImage (IImage image, Rect frame, double alpha = 1.0)
		{
			NextCanvas.DrawImage (image, frame, alpha);
		}

		#endregion
	}

	public class StyleFilterCanvas : FilterCanvas
	{
		public StyleFilterCanvas (ICanvas nextCanvas)
			: base (nextCanvas)
		{
			
		}
		public virtual Pen GetPen (Pen pen)
		{
			return pen;
		}
		public virtual Brush GetBrush (Brush brush)
		{
			return brush;
		}
		public override void DrawPath (System.Collections.Generic.IEnumerable<PathOp> ops, Pen pen, Brush brush)
		{
			base.DrawPath (ops, GetPen (pen), GetBrush (brush));
		}
		public override void DrawRectangle (Rect frame, Pen pen, Brush brush)
		{
			base.DrawRectangle (frame, GetPen (pen), GetBrush (brush));
		}
		public override void DrawEllipse (Rect frame, Pen pen, Brush brush)
		{
			base.DrawEllipse (frame, GetPen (pen), GetBrush (brush));
		}
		public override void DrawText (string text, Rect frame, Font font, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null)
		{
			base.DrawText (text, frame, font, alignment, GetPen (pen), GetBrush (brush));
		}
	}
}

