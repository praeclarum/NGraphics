using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Text;

namespace NGraphics
{
	public class AndroidPlatform : IPlatform
	{
		public string Name { get { return "Android"; } }

		public IImageCanvas CreateImageCanvas (Size size, double scale = 1.0, bool transparency = true)
		{
			var pixelWidth = (int)Math.Ceiling (size.Width * scale);
			var pixelHeight = (int)Math.Ceiling (size.Height * scale);
			var bitmap = Bitmap.CreateBitmap (pixelWidth, pixelHeight, Bitmap.Config.Argb8888);
			return new BitmapCanvas (bitmap, scale);
		}
	}

	public class BitmapImage : IImage
	{
		readonly Bitmap bitmap;
//		readonly double scale;

		public BitmapImage (Bitmap bitmap, double scale = 1.0)
		{
			this.bitmap = bitmap;
//			this.scale = scale;
		}

		public void SaveAsPng (string path)
		{
			using (var f = System.IO.File.OpenWrite (path)) {
				bitmap.Compress (Bitmap.CompressFormat.Png, 100, f);
			}
		}
	}

	public class BitmapCanvas : CanvasCanvas, IImageCanvas
	{
		readonly Bitmap bitmap;
		readonly double scale;

		public BitmapCanvas (Bitmap bitmap, double scale = 1.0)
			: base (new Canvas (bitmap))
		{
			this.bitmap = bitmap;
			this.scale = scale;

			graphics.Scale ((float)scale, (float)scale);
		}

		public IImage GetImage ()
		{
			return new BitmapImage (bitmap, scale);
		}
	}

	public class CanvasCanvas : ICanvas
	{
		protected readonly Canvas graphics;

		public CanvasCanvas (Canvas graphics)
		{
			this.graphics = graphics;
		}

		public void SaveState ()
		{
			graphics.Save (SaveFlags.Matrix|SaveFlags.Clip);
		}
		public void Transform (Transform transform)
		{
			var t = transform;
			var stack = new Stack<Transform> ();
			while (t != null) {
				stack.Push (t);
				t = t.Previous;
			}
			while (stack.Count > 0) {
				t = stack.Pop ();

				var rt = t as Rotate;
				if (rt != null) {
					graphics.Rotate ((float)rt.Angle);
					t = t.Previous;
					continue;
				}
				var tt = t as Translate;
				if (tt != null) {
					graphics.Translate ((float)tt.Size.Width, (float)tt.Size.Height);
					t = t.Previous;
					continue;
				}
				throw new NotSupportedException ("Transform " + t);
			}
		}
		public void RestoreState ()
		{
			graphics.Restore ();
		}

		TextPaint GetFontPaint (TextAlignment alignment)
		{
			var paint = new TextPaint (PaintFlags.AntiAlias);
			paint.TextAlign = Paint.Align.Left;
			if (alignment == TextAlignment.Center)
				paint.TextAlign = Paint.Align.Left;
			else if (alignment == TextAlignment.Right)
				paint.TextAlign = Paint.Align.Right;
			return paint;
		}
		Paint GetPenPaint (Pen pen)
		{
			var paint = new Paint (PaintFlags.AntiAlias);
			paint.SetStyle (Paint.Style.Stroke);
			paint.SetARGB (pen.Color.AlphaByte, pen.Color.RedByte, pen.Color.GreenByte, pen.Color.BlueByte);
			paint.StrokeWidth = (float)pen.Width;
			return paint;
		}
		Paint GetBrushPaint (Brush brush, Rect frame)
		{
			var paint = new Paint (PaintFlags.AntiAlias);
			AddBrushPaint (paint, brush, frame);
			return paint;
		}
		void AddBrushPaint (Paint paint, Brush brush, Rect bb)
		{
			paint.SetStyle (Paint.Style.Fill);

			var sb = brush as SolidBrush;
			if (sb != null) {
				paint.SetARGB (sb.Color.AlphaByte, sb.Color.RedByte, sb.Color.GreenByte, sb.Color.BlueByte);
				return;
			}

			var lgb = brush as LinearGradientBrush;
			if (lgb != null) {
				var n = lgb.Stops.Count;
				var locs = new float [n];
				var comps = new int [n];
				for (var i = 0; i < n; i++) {
					var s = lgb.Stops [i];
					locs [i] = (float)s.Offset;
					comps [i] = s.Color.Argb;
				}
				var p1 = bb.Position + lgb.RelativeStart * bb.Size;
				var p2 = bb.Position + lgb.RelativeEnd * bb.Size;
				var lg = new LinearGradient (
					(float)p1.X, (float)p1.Y,
					(float)p2.X, (float)p2.Y,
					comps,
					locs,
					Shader.TileMode.Clamp);

				paint.SetShader (lg);
				return;
			}

			var rgb = brush as RadialGradientBrush;
			if (rgb != null) {
				var n = rgb.Stops.Count;
				var locs = new float [n];
				var comps = new int [n];
				for (var i = 0; i < n; i++) {
					var s = rgb.Stops [i];
					locs [i] = (float)s.Offset;
					comps [i] = s.Color.Argb;
				}
				var p1 = bb.Position + rgb.RelativeCenter * bb.Size;
				var r = rgb.RelativeRadius * bb.Size;
				var rg = new RadialGradient (
					(float)p1.X, (float)p1.Y,
					(float)r.Max,
					comps,
					locs,
					Shader.TileMode.Clamp);

				paint.SetShader (rg);
				return;
			}

			throw new NotSupportedException ("Brush " + brush);
		}

		public void DrawText (string text, Rect frame, Font font, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null)
		{
			if (brush == null)
				return;

			if (frame.Width < double.MaxValue) {
				var paint = GetFontPaint (alignment);

				var align = global::Android.Text.Layout.Alignment.AlignNormal;
				if (alignment == TextAlignment.Center)
					align = global::Android.Text.Layout.Alignment.AlignCenter;
				else if (alignment == TextAlignment.Right)
					align = global::Android.Text.Layout.Alignment.AlignOpposite;

				var sl = new global::Android.Text.StaticLayout (text, paint, (int)Math.Floor (frame.Width), align, 1, 0, false);

				sl.Draw (graphics);
			}
//			else {
//				var paint = GetFontPaint ();
//				var w = paint.MeasureText (text);
//				var fm = paint.GetFontMetrics ();
//				var h = fm.Ascent + fm.Descent;
//				var fr = new Rect (point, new Size (w, h));
//				AddBrushPaint (paint, brush, fr);
//				graphics.DrawText (text, (float)point.X, (float)point.Y, paint);
//			}
		}
		public void DrawPath (IEnumerable<PathOp> ops, Pen pen = null, Brush brush = null)
		{
			if (pen == null && brush == null)
				return;

			using (var path = new global::Android.Graphics.Path ()) {

				var bb = new BoundingBoxBuilder ();

				foreach (var op in ops) {
					var mt = op as MoveTo;
					if (mt != null) {
						var p = mt.Point;
						path.MoveTo ((float)p.X, (float)p.Y);
						bb.Add (p);
						continue;
					}
					var lt = op as LineTo;
					if (lt != null) {
						var p = lt.Point;
						path.LineTo ((float)p.X, (float)p.Y);
						bb.Add (p);
						continue;
					}
                    var ct = op as CurveTo;
                    if (ct != null) {
                        var p = ct.Point;
                        var c1 = ct.Control1;
                        var c2 = ct.Control2;
						path.CubicTo ((float)c1.X, (float)c1.Y, (float)c2.X, (float)c2.Y, (float)p.X, (float)p.Y);
						bb.Add (p);
						bb.Add (c1);
						bb.Add (c2);
                        continue;
                    }
                    var cp = op as ClosePath;
					if (cp != null) {
						path.Close ();
						continue;
					}

					throw new NotSupportedException ("Path Op " + op);
				}

				var frame = bb.BoundingBox;

				if (brush != null) {
					var paint = GetBrushPaint (brush, frame);
					graphics.DrawPath (path, paint);
				}
				if (pen != null) {
					var paint = GetPenPaint (pen);
					graphics.DrawPath (path, paint);
				}
			}
		}
		public void DrawRectangle (Rect frame, Pen pen = null, Brush brush = null)
		{
			if (brush != null) {
				var paint = GetBrushPaint (brush, frame);
				graphics.DrawRect ((float)(frame.X), (float)(frame.Y), (float)(frame.X + frame.Width), (float)(frame.Y + frame.Height), paint);
			}
			if (pen != null) {
				var paint = GetPenPaint (pen);
				graphics.DrawRect ((float)(frame.X), (float)(frame.Y), (float)(frame.X + frame.Width), (float)(frame.Y + frame.Height), paint);
			}

		}
		public void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null)
		{
			if (brush != null) {
				var paint = GetBrushPaint (brush, frame);
				graphics.DrawOval (Conversions.GetRectangle (frame), paint);
			}
			if (pen != null) {
				var paint = GetPenPaint (pen);
				graphics.DrawOval (Conversions.GetRectangle (frame), paint);
			}
		}
	}

	public static class Conversions
	{
		public static System.Drawing.Color GetColor (this Color color)
		{
			return System.Drawing.Color.FromArgb (color.AlphaByte, color.RedByte, color.GreenByte, color.BlueByte);
		}

		public static PointF ToPointF (Point point)
        {
            return new PointF ((float)point.X, (float)point.Y);
        }

		public static RectF GetRectangle (Rect frame)
		{
			return new RectF ((float)frame.X, (float)frame.Y, (float)(frame.X + frame.Width), (float)(frame.Y + frame.Height));
		}
	}
}

