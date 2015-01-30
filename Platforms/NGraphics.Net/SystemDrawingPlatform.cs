using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace NGraphics
{
	public class SystemDrawingPlatform : IPlatform
	{
		public string Name { get { return "Net"; } }

		public IImageCanvas CreateImageSurface (int pixelWidth, int pixelHeight, bool transparency = true)
		{
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
				graphics.FillRectangle (brush.GetBrush (), Conversions.GetRectangle (frame));
			}
			if (pen != null) {
				var r = Conversions.GetRectangle (frame);
				graphics.DrawRectangle (pen.GetPen (), r.X, r.Y, r.Width, r.Height);
			}
		}
		public void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null)
		{
			if (brush != null) {
				graphics.FillEllipse (brush.GetBrush (), Conversions.GetRectangle (frame));
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

		public static System.Drawing.Brush GetBrush (this Brush brush)
		{
			var cb = brush as SolidBrush;
			if (cb != null) {
				return new System.Drawing.SolidBrush (cb.Color.GetColor ());
			}
			return new System.Drawing.SolidBrush (System.Drawing.Color.White);
		}

		public static RectangleF GetRectangle (Rect frame)
		{
			return new RectangleF ((float)frame.X, (float)frame.Y, (float)frame.Width, (float)frame.Height);
		}
	}
}

