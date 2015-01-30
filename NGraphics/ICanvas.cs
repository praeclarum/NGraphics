using System;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public interface ICanvas
	{
		void DrawRectangle (Rect frame, Pen pen = null, Brush brush = null);
		void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null);
	}

	public static class CanvasEx
	{
		public static void DrawRectangle (this ICanvas surface, Point position, Size size, Pen pen = null, Brush brush = null)
		{
			surface.DrawRectangle (new Rect (position, size), pen, brush);
		}

		public static void DrawEllipse (this ICanvas surface, Point position, Size size, Pen pen = null, Brush brush = null)
		{
			surface.DrawEllipse (new Rect (position, size), pen, brush);
		}
	}
}
