using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace NGraphics
{
	public class SystemDrawingPlatform : IPlatform
	{
		public string Name { get { return "Net"; } }

		public IImageCanvas CreateImageCanvas (Size size, double scale = 1.0, bool transparency = true)
		{
			var pixelWidth = (int)Math.Ceiling (size.Width * scale);
			var pixelHeight = (int)Math.Ceiling (size.Height * scale);
			var format = transparency ? PixelFormat.Format32bppPArgb : PixelFormat.Format24bppRgb;
			var bitmap = new Bitmap (pixelWidth, pixelHeight, format);
			return new BitmapSurface (bitmap, scale);
		}

		public IImage CreateImage (Color[,] colors, double scale = 1.0)
		{
			var pixelWidth = colors.GetLength (0);
			var pixelHeight = colors.GetLength (1);
			var format = PixelFormat.Format32bppArgb;
			Bitmap bitmap;
			unsafe {
				fixed (Color *c = colors) {
					bitmap = new Bitmap (pixelWidth, pixelHeight, pixelWidth*4, format, new IntPtr (c));
				}
			}
			return new BitmapImage (bitmap);
		}
	}

	public class BitmapImage : IImage
	{
		Bitmap bitmap;

		public BitmapImage (Bitmap bitmap)
		{
			this.bitmap = bitmap;
		}

		public void SaveAsPng (string path)
		{
			bitmap.Save (path, ImageFormat.Png);
		}

//		public void Draw (ISurface surface)
//		{
//			surface.DrawImage ();
//		}
	}

	public class BitmapSurface : GraphicsSurface, IImageCanvas
	{
		Bitmap bitmap;
		readonly double scale;

		public BitmapSurface (Bitmap bitmap, double scale = 1.0)
			: base (Graphics.FromImage (bitmap))
		{
			this.bitmap = bitmap;
			this.scale = scale;

			graphics.ScaleTransform ((float)scale, (float)scale);
		}

		public IImage GetImage ()
		{
			return new BitmapImage (bitmap);
		}
	}

	public class GraphicsSurface : ICanvas
	{
		protected readonly Graphics graphics;
		readonly Stack<GraphicsState> stateStack = new Stack<GraphicsState> ();

		public GraphicsSurface (Graphics graphics)
		{
			this.graphics = graphics;

			graphics.SmoothingMode = SmoothingMode.HighQuality;
		}

		public void SaveState ()
		{
			var s = graphics.Save ();
			stateStack.Push (s);
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
					graphics.RotateTransform ((float)rt.Angle);
					t = t.Previous;
					continue;
				}
				var tt = t as Translate;
				if (tt != null) {
					graphics.TranslateTransform ((float)tt.Size.Width, (float)tt.Size.Height);
					t = t.Previous;
					continue;
				}
                var st = t as Scale;
                if (st != null) {
                    graphics.ScaleTransform ((float)st.Size.Width, (float)st.Size.Height);
                    t = t.Previous;
                    continue;
                }
                throw new NotSupportedException ("Transform " + t);
			}
		}
		public void RestoreState ()
		{
			if (stateStack.Count > 0) {
				var s = stateStack.Pop ();
				graphics.Restore (s);
			}
		}

		public void DrawText (string text, Rect frame, Font font, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null)
		{
			if (brush == null)
				return;
			var sdfont = new System.Drawing.Font ("Georgia", 16);
			var sz = graphics.MeasureString (text, sdfont);
			var point = frame.Position;
			var fr = new Rect (point, new Size (sz.Width, sz.Height));
			graphics.DrawString (text, sdfont, Conversions.GetBrush (brush, fr), Conversions.ToPointF (point));
		}
		public void DrawPath (IEnumerable<PathOp> ops, Pen pen = null, Brush brush = null)
		{
			using (var path = new GraphicsPath ()) {

				Rect bb = new Rect ();
				var nbb = 0;

				var position = Point.Zero;

				foreach (var op in ops) {
					var mt = op as MoveTo;
					if (mt != null) {
						var p = mt.Point;
						position = p;
						if (nbb == 0)
							bb = new Rect (p, Size.Zero);
						else
							bb = bb.Union (p);
						nbb++;
						continue;
					}
					var lt = op as LineTo;
					if (lt != null) {
						var p = lt.Point;
						path.AddLine (Conversions.ToPointF (position), Conversions.ToPointF (p));
						position = p;
						if (nbb == 0)
							bb = new Rect (p, Size.Zero);
						else
							bb = bb.Union (p);
						nbb++;
						continue;
					}
                    var ct = op as CurveTo;
                    if (ct != null) {
                        var p = ct.Point;
                        var c1 = ct.Control1;
                        var c2 = ct.Control2;
                        path.AddBezier (Conversions.ToPointF (position), Conversions.ToPointF (c1),
                            Conversions.ToPointF (c2), Conversions.ToPointF (p));
                        position = p;
                        if (nbb == 0)
                            bb = new Rect (p, Size.Zero);
                        bb = bb.Union (p).Union (c1).Union (c2);
                        nbb++;
                        continue;
                    }
                    var cp = op as ClosePath;
					if (cp != null) {
						path.CloseFigure ();
						continue;
					}

					throw new NotSupportedException ("Path Op " + op);
				}

				var frame = bb;
				if (brush != null) {
					graphics.FillPath (brush.GetBrush (frame), path);
				}
				if (pen != null) {
					var r = Conversions.GetRectangle (frame);
					graphics.DrawPath (pen.GetPen (), path);
				}
			}
		}
		public void DrawRectangle (Rect frame, Pen pen = null, Brush brush = null)
		{
			if (brush != null) {
				graphics.FillRectangle (brush.GetBrush (frame), Conversions.GetRectangle (frame));
			}
			if (pen != null) {
				var r = Conversions.GetRectangle (frame);
				graphics.DrawRectangle (pen.GetPen (), r.X, r.Y, r.Width, r.Height);
			}
		}
		public void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null)
		{
			if (brush != null) {
				graphics.FillEllipse (brush.GetBrush (frame), Conversions.GetRectangle (frame));
			}
			if (pen != null) {
				graphics.DrawEllipse (pen.GetPen (), Conversions.GetRectangle (frame));
			}
		}
	}

	public static class Conversions
	{
		public static System.Drawing.Color GetColor (this Color color)
		{
			return System.Drawing.Color.FromArgb (color.A, color.R, color.G, color.B);
		}

		public static System.Drawing.Pen GetPen (this Pen pen)
		{
			return new System.Drawing.Pen (GetColor (pen.Color), (float)pen.Width);
		}

		public static System.Drawing.Brush GetBrush (this Brush brush, Rect frame)
		{
			var cb = brush as SolidBrush;
			if (cb != null) {
				return new System.Drawing.SolidBrush (cb.Color.GetColor ());
			}
            var lgb = brush as LinearGradientBrush;
            if (lgb != null && lgb.Stops.Count >= 2) {
                var s = frame.Position + lgb.RelativeStart * frame.Size;
                var e = frame.Position + lgb.RelativeEnd * frame.Size;
                var s1 = lgb.Stops[0];
                var s2 = lgb.Stops[lgb.Stops.Count - 1];
                var b = new System.Drawing.Drawing2D.LinearGradientBrush (ToPointF (s), ToPointF (e), GetColor (s1.Color), GetColor (s2.Color));

                // Build the blend
                var n = lgb.Stops.Count;
                var an = 0;
                if (s1.Offset != 0) {
                    an++;
                }
                if (s2.Offset != 1) {
                    an++;
                }
                var blend = new System.Drawing.Drawing2D.ColorBlend (n + an);
                var o = 0;
                if (s1.Offset != 0) {
                    blend.Colors[0] = GetColor (s1.Color);
                    blend.Positions[0] = 0;
                    o = 1;
                }
                for (var i = 0; i < n; i++) {
                    blend.Colors[i + o] = GetColor (lgb.Stops[i].Color);
                    blend.Positions[i + o] = (float)lgb.Stops[i].Offset;
                }
                if (s2.Offset != 1) {
                    blend.Colors[n + an - 1] = GetColor (s1.Color);
                    blend.Positions[n + an - 1] = 1;
                }
                b.InterpolationColors = blend;

                // This is what we want but it doesn't work
                //b.WrapMode = System.Drawing.Drawing2D.WrapMode.Clamp;

                return b;
            }
			return new System.Drawing.SolidBrush (System.Drawing.Color.Black);
		}

        public static PointF ToPointF (Point point)
        {
            return new PointF ((float)point.X, (float)point.Y);
        }

		public static RectangleF GetRectangle (Rect frame)
		{
			return new RectangleF ((float)frame.X, (float)frame.Y, (float)frame.Width, (float)frame.Height);
		}
	}
}

