using System;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public interface ICanvas
	{
		void DrawEllipse (Rectangle frame, Pen pen = null, Brush brush = null);
	}

	public static class CanvasEx
	{
		public static void DrawEllipse (this ICanvas surface, Point position, Size size, Pen pen = null, Brush brush = null)
		{
			surface.DrawEllipse (new Rectangle (position, size), pen, brush);
		}
	}
}
