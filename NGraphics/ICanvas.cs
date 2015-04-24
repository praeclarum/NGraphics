using System;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public interface ICanvas
	{
		void SaveState ();
		void Transform (Transform transform);
		void RestoreState ();

		Size MeasureText(string text, Font font);
		void DrawText (string text, Rect frame, Font font, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null);
		void DrawPath (IEnumerable<PathOp> ops, Pen pen = null, Brush brush = null);
		void DrawRectangle (Rect frame, Pen pen = null, Brush brush = null);
		void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null);
		void DrawImage (IImage image, Rect frame, double alpha = 1.0);
	}

	public static class CanvasEx
	{
		public static void Translate (this ICanvas canvas, double dx, double dy)
		{
			canvas.Transform (Transform.Translate (dx, dy));
		}
		public static void Translate (this ICanvas canvas, Size translation)
		{
			canvas.Transform (Transform.Translate (translation));
		}
		public static void Translate (this ICanvas canvas, Point translation)
		{
			canvas.Transform (Transform.Translate (translation));
		}
		/// <summary>
		/// Rotate the specified canvas by the given angle (in degrees).
		/// </summary>
		/// <param name="angle">Angle in degrees.</param>
		public static void Rotate (this ICanvas canvas, double angle)
		{
			if (angle != 0) {
				canvas.Transform (Transform.Rotate (angle));
			}
		}
		/// <param name="angle">Angle in degrees.</param>
		public static void Rotate (this ICanvas canvas, double angle, Point point)
		{
			if (angle != 0) {
				canvas.Translate (point);
				canvas.Rotate (angle);
				canvas.Translate (-point);
			}
		}
		/// <param name="angle">Angle in degrees.</param>
		public static void Rotate (this ICanvas canvas, double angle, double x, double y)
		{
			if (angle != 0) {
				canvas.Rotate (angle, new Point (x, y));
			}
		}

		public static void Scale (this ICanvas canvas, double sx, double sy)
		{
			if (sx != 1 || sy != 1) {
				canvas.Transform (Transform.Scale (sx, sy));
			}
		}
		public static void Scale (this ICanvas canvas, double scale)
		{
			if (scale != 1) {
				canvas.Transform (Transform.Scale (scale, scale));
			}
		}
		public static void Scale (this ICanvas canvas, Size scale)
		{
			if (scale.Width != 1 || scale.Height != 1) {
				canvas.Transform (Transform.Scale (scale));
			}
		}
		public static void Scale (this ICanvas canvas, Size scale, Point point)
		{
			if (scale.Width != 1 || scale.Height != 1) {
				canvas.Translate (point);
				canvas.Scale (scale);
				canvas.Translate (-point);
			}
		}
		public static void Scale (this ICanvas canvas, double scale, Point point)
		{
			if (scale != 1) {
				canvas.Scale (new Size (scale), point);
			}
		}
		public static void Scale (this ICanvas canvas, double sx, double sy, double x, double y)
		{
			if (sx != 1 || sy != 1) {
				canvas.Scale (new Size (sx, sy), new Point (x, y));
			}
		}
		public static void Scale (this ICanvas canvas, double scale, double x, double y)
		{
			if (scale != 1) {
				canvas.Scale (new Size (scale), new Point (x, y));
			}
		}


		public static void DrawRectangle (this ICanvas canvas, double x, double y, double width, double height, Pen pen = null, Brush brush = null)
		{
			canvas.DrawRectangle (new Rect (x, y, width, height), pen, brush);
		}
		public static void DrawRectangle (this ICanvas canvas, Point position, Size size, Pen pen = null, Brush brush = null)
		{
			canvas.DrawRectangle (new Rect (position, size), pen, brush);
		}
		public static void FillRectangle (this ICanvas canvas, Rect frame, Color color)
		{
			canvas.DrawRectangle (frame, brush: new SolidBrush (color));
		}
		public static void FillRectangle (this ICanvas canvas, double x, double y, double width, double height, Color color)
		{
			canvas.DrawRectangle (new Rect (x, y, width, height), brush: new SolidBrush (color));
		}
		public static void FillRectangle (this ICanvas canvas, double x, double y, double width, double height, Brush brush)
		{
			canvas.DrawRectangle (new Rect (x, y, width, height), brush: brush);
		}
		public static void FillRectangle (this ICanvas canvas, Rect frame, Brush brush)
		{
			canvas.DrawRectangle (frame, brush: brush);
		}
		public static void StrokeRectangle (this ICanvas canvas, Rect frame, Color color, double width = 1.0)
		{
			canvas.DrawRectangle (frame, pen: new Pen (color, width));
		}

		public static void DrawEllipse (this ICanvas canvas, double x, double y, double width, double height, Pen pen = null, Brush brush = null)
		{
			canvas.DrawEllipse (new Rect (x, y, width, height), pen, brush);
		}
		public static void DrawEllipse (this ICanvas canvas, Point position, Size size, Pen pen = null, Brush brush = null)
		{
			canvas.DrawEllipse (new Rect (position, size), pen, brush);
		}
		public static void FillEllipse (this ICanvas canvas, Point position, Size size, Brush brush)
		{
			canvas.DrawEllipse (new Rect (position, size), null, brush);
		}
		public static void FillEllipse (this ICanvas canvas, Point position, Size size, Color color)
		{
			canvas.DrawEllipse (new Rect (position, size), null, new SolidBrush (color));
		}
		public static void FillEllipse (this ICanvas canvas, double x, double y, double width, double height, Color color)
		{
			canvas.DrawEllipse (new Rect (x, y, width, height), brush: new SolidBrush (color));
		}
		public static void FillEllipse (this ICanvas canvas, double x, double y, double width, double height, Brush brush)
		{
			canvas.DrawEllipse (new Rect (x, y, width, height), brush: brush);
		}
		public static void FillEllipse (this ICanvas canvas, Rect frame, Color color)
		{
			canvas.DrawEllipse (frame, brush: new SolidBrush (color));
		}
		public static void FillEllipse (this ICanvas canvas, Rect frame, Brush brush)
		{
			canvas.DrawEllipse (frame, brush: brush);
		}
		public static void StrokeEllipse (this ICanvas canvas, Rect frame, Color color, double width = 1.0)
		{
			canvas.DrawEllipse (frame, pen: new Pen (color, width));
		}
		public static void StrokeEllipse (this ICanvas canvas, Point position, Size size, Color color, double width = 1.0)
		{
			canvas.DrawEllipse (new Rect (position, size), new Pen (color, width), null);
		}

		public static void DrawPath (this ICanvas canvas, Action<Path> draw, Pen pen = null, Brush brush = null)
		{
			var p = new Path (pen, brush);
			draw (p);
			p.Draw (canvas);
		}
		public static void FillPath (this ICanvas canvas, IEnumerable<PathOp> ops, Brush brush)
		{
			canvas.DrawPath (ops, brush: brush);
		}
		public static void FillPath (this ICanvas canvas, IEnumerable<PathOp> ops, Color color)
		{
			canvas.DrawPath (ops, brush: new SolidBrush (color));
		}
		public static void FillPath (this ICanvas canvas, Action<Path> draw, Brush brush)
		{
			var p = new Path (null, brush);
			draw (p);
			p.Draw (canvas);
		}
		public static void FillPath (this ICanvas canvas, Action<Path> draw, Color color)
		{
			FillPath (canvas, draw, new SolidBrush (color));
		}

		public static void DrawLine (this ICanvas canvas, Point start, Point end, Pen pen)
		{
			var p = new Path { Pen = pen };
			p.MoveTo (start);
			p.LineTo (end);
			p.Draw (canvas);				
		}
		public static void DrawLine (this ICanvas canvas, Point start, Point end, Color color, double width = 1.0)
		{
			var p = new Path { Pen = new Pen (color, width) };
			p.MoveTo (start);
			p.LineTo (end);
			p.Draw (canvas);				
		}
		public static void DrawLine (this ICanvas canvas, double x1, double y1, double x2, double y2, Color color, double width = 1.0)
		{
			var p = new Path { Pen = new Pen (color, width) };
			p.MoveTo (x1, y1);
			p.LineTo (x2, y2);
			p.Draw (canvas);				
		}

		public static void DrawImage (this ICanvas canvas, IImage image)
		{
			canvas.DrawImage (image, new Rect (image.Size));
		}

		public static void DrawImage (this ICanvas canvas, IImage image, double x, double y, double width, double height, double alpha = 1.0)
		{
			canvas.DrawImage (image, new Rect (x, y, width, height), alpha);
		}

		public static void DrawText (this ICanvas canvas, string text, Point point, Font font, Brush brush)
		{
			canvas.DrawText (text, new Rect (point, Size.MaxValue), font, brush: brush);
		}

		public static void DrawText (this ICanvas canvas, string text, Point point, Font font, Color color)
		{
			canvas.DrawText (text, new Rect (point, Size.MaxValue), font, brush: new SolidBrush (color));
		}
	}
}
