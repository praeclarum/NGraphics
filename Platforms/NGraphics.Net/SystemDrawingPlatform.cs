﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;

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
			return new BitmapCanvas (bitmap, scale);
		}

		public IImage LoadImage (Stream stream)
		{
			var image = Image.FromStream (stream);
			return new ImageImage (image);
		}

		public IImage LoadImage (string path)
		{
			var image = Image.FromFile (path);
			return new ImageImage (image);
		}

		public IImage CreateImage (Color[] colors, int width, double scale = 1.0)
		{
			var pixelWidth = width;
			var pixelHeight = colors.Length / width;
			var format = PixelFormat.Format32bppArgb;
			Bitmap bitmap;
			unsafe {
				fixed (Color *c = colors) {
					bitmap = new Bitmap (pixelWidth, pixelHeight, pixelWidth*4, format, new IntPtr (c));
				}
			}
			return new ImageImage (bitmap);
		}
	}

	public class ImageImage : IImage
	{
		readonly Image image;

		public Image Image {
			get {
				return image;
			}
		}

		public ImageImage (Image image)
		{
			this.image = image;
		}

		public void SaveAsPng (string path)
		{
			image.Save (path, ImageFormat.Png);
		}

		public void SaveAsPng (Stream stream)
		{
			image.Save (stream, ImageFormat.Png);
		}

		public Size Size
		{
			get
			{
				return new Size(image.Width, image.Height);
			}
		}

		public double Scale
		{
			get
			{
				return 1;
			}
		}
	}

	public class BitmapCanvas : GraphicsCanvas, IImageCanvas
	{
		readonly Bitmap bitmap;
		readonly double scale;

		public Size Size { get { return new Size (bitmap.Width / scale, bitmap.Height / scale); } }
		public double Scale { get { return scale; } }

		public BitmapCanvas (Bitmap bitmap, double scale = 1.0)
			: base (Graphics.FromImage (bitmap))
		{
			this.bitmap = bitmap;
			this.scale = scale;

			graphics.ScaleTransform ((float)scale, (float)scale);
		}

		public IImage GetImage ()
		{
			return new ImageImage (bitmap);
		}
	}

	public class GraphicsCanvas : ICanvas
	{
		protected readonly Graphics graphics;
		readonly Stack<GraphicsState> stateStack = new Stack<GraphicsState> ();

		public GraphicsCanvas (Graphics graphics)
		{
			this.graphics = graphics;

            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
		}

		public void SaveState ()
		{
			var s = graphics.Save ();
			stateStack.Push (s);
		}
		public void Transform (Transform transform)
		{
			try {
				graphics.MultiplyTransform (new Matrix (
					(float)transform.A, (float)transform.B,
					(float)transform.C, (float)transform.D,
					(float)transform.E, (float)transform.F), MatrixOrder.Prepend);
			}
			catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}
		public void RestoreState ()
		{
			if (stateStack.Count > 0) {
				var s = stateStack.Pop ();
				graphics.Restore (s);
			}
		}

		public Size MeasureText(string text, Font font)
		{
			using (var netFont = new System.Drawing.Font (font.Name, (float)font.Size, FontStyle.Regular, GraphicsUnit.Point))
			{
				var result = graphics.MeasureString(text, netFont);
				return new Size(result.Width, result.Height);
			}
		}

		public void DrawText (string text, Rect frame, Font font, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null)
		{
			if (brush == null)
				throw new ArgumentNullException(nameof(brush) );
			var sdfont = new System.Drawing.Font (font.Family, (float)font.Size, FontStyle.Regular, GraphicsUnit.Point);
			var sz = graphics.MeasureString (text, sdfont);
			var point = frame.Position;
            var fr = new Rect (point, new Size (sz.Width, sz.Height));
			graphics.DrawString (text, sdfont, Conversions.GetBrush (brush, fr), Conversions.GetPointF (point));
		}
		public void DrawPath (IEnumerable<PathOp> ops, Pen pen = null, Brush brush = null)
		{
			using (var path = new GraphicsPath ()) {

                var bb = new BoundingBoxBuilder ();

				var position = Point.Zero;

				foreach (var op in ops) {
					var mt = op as MoveTo;
					if (mt != null) {
						var p = mt.Point;
						position = p;
                        bb.Add (p);
						continue;
					}
					var lt = op as LineTo;
					if (lt != null) {
						var p = lt.Point;
						path.AddLine (Conversions.GetPointF (position), Conversions.GetPointF (p));
						position = p;
                        bb.Add (p);
                        continue;
					}
                    var at = op as ArcTo;
                    if (at != null) {
                        var p = at.Point;
                        path.AddLine (Conversions.GetPointF (position), Conversions.GetPointF (p));
                        position = p;
                        bb.Add (p);
                        continue;
                    }
                    var ct = op as CurveTo;
                    if (ct != null) {
                        var p = ct.Point;
                        var c1 = ct.Control1;
                        var c2 = ct.Control2;
                        path.AddBezier (Conversions.GetPointF (position), Conversions.GetPointF (c1),
                            Conversions.GetPointF (c2), Conversions.GetPointF (p));
                        position = p;
                        bb.Add (p);
                        bb.Add (c1);
                        bb.Add (c2);
                        continue;
                    }
                    var cp = op as ClosePath;
					if (cp != null) {
						path.CloseFigure ();
						continue;
					}

					throw new NotSupportedException ("Path Op " + op);
				}

				var frame = bb.BoundingBox;
				if (brush != null) {
					graphics.FillPath (brush.GetBrush (frame), path);
				}
				if (pen != null) {
					var r = Conversions.GetRectangleF (frame);
					graphics.DrawPath (pen.GetPen (), path);
				}
			}
		}
		public void DrawRectangle (Rect frame, Pen pen = null, Brush brush = null)
		{
			if (brush != null) {
				graphics.FillRectangle (brush.GetBrush (frame), Conversions.GetRectangleF (frame));
			}
			if (pen != null) {
				var r = Conversions.GetRectangleF (frame);
				graphics.DrawRectangle (pen.GetPen (), r.X, r.Y, r.Width, r.Height);
			}
		}
		public void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null)
		{
			if (brush != null) {
				graphics.FillEllipse (brush.GetBrush (frame), Conversions.GetRectangleF (frame));
			}
			if (pen != null) {
				graphics.DrawEllipse (pen.GetPen (), Conversions.GetRectangleF (frame));
			}
		}
		public void DrawImage (IImage image, Rect frame, double alpha = 1.0)
		{
			var ii = image as ImageImage;
			if (ii != null) {
				if (alpha < 0.999) {
					var i = new ImageAttributes ();
					var mat = new ColorMatrix (new float[][] { 
						new[] { 1.0f, 0.0f, 0.0f, 0.0f, 0.0f },
						new[] { 0.0f, 1.0f, 0.0f, 0.0f, 0.0f },
						new[] { 0.0f, 0.0f, 1.0f, 0.0f, 0.0f },
						new[] { 0.0f, 0.0f, 0.0f, (float)alpha, 0.0f },
						new[] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f }
					});
					i.SetColorMatrix (mat);
					var size = ii.Image.Size;
					graphics.DrawImage (ii.Image, Conversions.GetRectangle (frame),
						0, 0, size.Width, size.Height, GraphicsUnit.Pixel, i);
				} else {
					graphics.DrawImage (ii.Image, Conversions.GetRectangleF (frame));
				}
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

        static ColorBlend BuildBlend (List<GradientStop> stops, bool reverse = false)
        {
            if (stops.Count < 2)
                return null;

            var s1 = stops[0];
            var s2 = stops[stops.Count - 1];

            // Build the blend
            var n = stops.Count;
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
                blend.Colors[i + o] = GetColor (stops[i].Color);
                blend.Positions[i + o] = (float)stops[i].Offset;
            }
            if (s2.Offset != 1) {
                blend.Colors[n + an - 1] = GetColor (s1.Color);
                blend.Positions[n + an - 1] = 1;
            }

            if (reverse) {
                for (var i = 0; i < blend.Positions.Length; i++) {
                    blend.Positions[i] = 1 - blend.Positions[i];
                }
                Array.Reverse (blend.Positions);
                Array.Reverse (blend.Colors);
            }

            return blend;
        }

		public static System.Drawing.Brush GetBrush (this Brush brush, Rect frame)
		{
			var cb = brush as SolidBrush;
			if (cb != null) {
				return new System.Drawing.SolidBrush (cb.Color.GetColor ());
			}

            var lgb = brush as LinearGradientBrush;
            if (lgb != null) {
                var s = lgb.Absolute ? lgb.Start : frame.Position + lgb.Start * frame.Size;
                var e = lgb.Absolute ? lgb.End : frame.Position + lgb.End * frame.Size;
                var b = new System.Drawing.Drawing2D.LinearGradientBrush (GetPointF (s), GetPointF (e), System.Drawing.Color.Black, System.Drawing.Color.Black);
                var bb = BuildBlend (lgb.Stops);
                if (bb != null) {
                    b.InterpolationColors = bb;
                }
                return b;
            }

            var rgb = brush as RadialGradientBrush;
            if (rgb != null) {
                var r = rgb.GetAbsoluteRadius (frame);
                var c = rgb.GetAbsoluteCenter (frame);
                var path = new GraphicsPath ();
                path.AddEllipse (GetRectangleF (new Rect (c - r, 2 * r)));
                var b = new PathGradientBrush (path);
                var bb = BuildBlend (rgb.Stops, true);
                if (bb != null) {
                    b.InterpolationColors = bb;
                }
                return b;
            }

			throw new NotImplementedException ("Brush " + brush);
		}

        public static PointF GetPointF (this Point point)
        {
            return new PointF ((float)point.X, (float)point.Y);
        }

		public static RectangleF GetRectangleF (this Rect frame)
		{
			return new RectangleF ((float)frame.X, (float)frame.Y, (float)frame.Width, (float)frame.Height);
		}

		public static System.Drawing.Rectangle GetRectangle (this Rect frame)
		{
			return new System.Drawing.Rectangle ((int)frame.X, (int)frame.Y, (int)frame.Width, (int)frame.Height);
		}
	}
}

