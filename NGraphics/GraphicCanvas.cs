using System;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{

	public class GraphicCanvas : ICanvas
	{
		public Graphic Graphic { get; private set; }

		public GraphicCanvas (Size size)
		{
			Graphic = new Graphic (size);
		}

		public void DrawPath (IEnumerable<PathCommand> commands, Pen pen = null, Brush brush = null)
		{
			Graphic.Children.Add (new Path (commands, pen, brush));
		}
		public void DrawRectangle (Rect frame, Pen pen = null, Brush brush = null)
		{
			Graphic.Children.Add (new Rectangle (frame, pen, brush));
		}
		public void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null)
		{
			Graphic.Children.Add (new Ellipse (frame, pen, brush));
		}
	}
}
