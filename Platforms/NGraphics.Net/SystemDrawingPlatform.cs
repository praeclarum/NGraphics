using System;
using System.Drawing;
using System.Drawing.Imaging;

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
			return new BitmapSurface (bitmap);
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

		public BitmapSurface (Bitmap bitmap)
			: base (Graphics.FromImage (bitmap))
		{
			this.bitmap = bitmap;
		}

		public IImage GetImage ()
		{
			return new BitmapImage (bitmap);
		}
	}

	public class GraphicsSurface : ICanvas
	{
		readonly Graphics graphics;

		public GraphicsSurface (Graphics graphics)
		{
			this.graphics = graphics;

			graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
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
			return System.Drawing.Color.FromArgb (color.AlphaByte, color.RedByte, color.GreenByte, color.BlueByte);
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

