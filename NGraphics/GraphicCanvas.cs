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

		public void DrawEllipse (Rectangle frame, Pen pen = null, Brush brush = null)
		{
			Graphic.Children.Add (new Ellipse (frame, pen, brush));
		}
	}
}
