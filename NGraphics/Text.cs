using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	public class Text : Element
	{
		public Point Point;
		public string String;

		public Text (Point point, string text, Pen pen = null, Brush brush = null)
			: base (pen, brush)
		{
			Point = point;
			String = text;
		}

		protected override void DrawElement (ICanvas canvas)
		{
			canvas.DrawText (Point, String, Pen, Brush);
		}
	}

}
