using System;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public class GraphicCanvas : ICanvas
	{
		readonly Stack<NGraphics.Transform> states = new Stack<NGraphics.Transform> ();

		public Graphic Graphic { get; private set; }

		public IPlatform TextPlatform { get; set; }

		public GraphicCanvas (Size size, IPlatform textPlatform)
		{
			states.Push (NGraphics.Transform.Identity);
			TextPlatform = textPlatform;
			Graphic = new Graphic (size);
		}

		Transform CurrentTransform {
			get {
				return states.Peek ();
			}
		}

		public void SaveState ()
		{
			states.Push (CurrentTransform);
		}
		public void Transform (Transform transform)
		{
			var t = states.Pop ();
			var nt = t * transform;
			states.Push (nt);
		}
		public void RestoreState ()
		{
			if (states.Count > 1) {
				states.Pop ();
			}
		}
		public TextMetrics MeasureText (string text, Font font)
		{
			return TextPlatform.MeasureText (text, font);
		}
		public void DrawText (string text, Rect frame, Font font, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null)
		{
			Graphic.Children.Add (new Text (text, frame, font, alignment, pen, brush) { Transform = CurrentTransform, });
		}
		public void DrawPath (IEnumerable<PathOp> commands, Pen pen = null, Brush brush = null)
		{
			Graphic.Children.Add (new Path (commands, pen, brush) { Transform = CurrentTransform, });
		}
		public void DrawRectangle (Rect frame, Size corner, Pen pen = null, Brush brush = null)
		{
			Graphic.Children.Add (new Rectangle (frame, corner, pen, brush) { Transform = CurrentTransform, });
		}
		public void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null)
		{
			Graphic.Children.Add (new Ellipse (frame, pen, brush) { Transform = CurrentTransform, });
		}
		public void DrawImage (IImage image, Rect frame, double alpha = 1.0)
		{
			throw new NotImplementedException ();
		}
	}
}
