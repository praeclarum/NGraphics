using System;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public interface ICanvas
	{
		void SaveState();
		void Transform (Transform transform);
		void RestoreState ();

		void DrawText (string text, Rect frame, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null);
		void DrawPath (IEnumerable<PathOp> ops, Pen pen = null, Brush brush = null);
		void DrawRectangle (Rect frame, Pen pen = null, Brush brush = null);
		void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null);
	}

	public static class CanvasEx
	{
		public static void Translate (this ICanvas canvas, double dx, double dy)
		{
			canvas.Transform (new Translate (dx, dy));
		}
		public static void Rotate (this ICanvas canvas, double angle)
		{
			canvas.Transform (new Rotate (angle));
		}
		public static void Scale (this ICanvas canvas, double sx, double sy)
		{
			canvas.Transform (new Scale (sx, sy));
		}

		public static void DrawRectangle (this ICanvas canvas, double x, double y, double width, double height, Pen pen = null, Brush brush = null)
		{
			canvas.DrawRectangle (new Rect (x, y, width, height), pen, brush);
		}
		public static void DrawRectangle (this ICanvas canvas, Point position, Size size, Pen pen = null, Brush brush = null)
		{
			canvas.DrawRectangle (new Rect (position, size), pen, brush);
		}

		public static void DrawEllipse (this ICanvas canvas, double x, double y, double width, double height, Pen pen = null, Brush brush = null)
		{
			canvas.DrawEllipse (new Rect (x, y, width, height), pen, brush);
		}
		public static void DrawEllipse (this ICanvas canvas, Point position, Size size, Pen pen = null, Brush brush = null)
		{
			canvas.DrawEllipse (new Rect (position, size), pen, brush);
		}
	}
}
