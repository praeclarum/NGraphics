using System;
using CoreGraphics;
using ImageIO;
using Foundation;

namespace NGraphics
{
	public class ApplePlatform : IPlatform
	{
		public string Name { 
			get { 
				#if __IOS__
				return "iOS"; 
				#else
				return "Mac";
				#endif
			} 
		}

		public IImageCanvas CreateImageSurface (int pixelWidth, int pixelHeight, bool transparency = true)
		{
			var bitmapInfo = transparency ? CGImageAlphaInfo.PremultipliedFirst : CGImageAlphaInfo.None;
			var bitsPerComp = 8;
			var bytesPerRow = transparency ? 4 * pixelWidth : 3 * pixelWidth;
			var colorSpace = CGColorSpace.CreateDeviceRGB ();
			var bitmap = new CGBitmapContext (IntPtr.Zero, pixelWidth, pixelHeight, bitsPerComp, bytesPerRow, colorSpace, bitmapInfo);
			return new CGBitmapContextSurface (bitmap);
		}
	}

	public class CGBitmapContextSurface : CGContextSurface, IImageCanvas
	{
		CGBitmapContext context;

		public CGBitmapContextSurface (CGBitmapContext context)
			: base (context)
		{
			this.context = context;
		}

		public IImage GetImage ()
		{
			return new CGImageImage (this.context.ToImage ());
		}
	}

	public class CGImageImage : IImage
	{
		CGImage image;

		public CGImageImage (CGImage image)
		{
			this.image = image;
		}

		public void SaveAsPng (string path)
		{
			var dest = CGImageDestination.Create (NSUrl.FromFilename (path), "public.png", 1);
			dest.AddImage (image);
			dest.Close ();
		}
	}

	public class CGContextSurface : ICanvas
	{
		CGContext context;

		public CGContextSurface (CGContext context)
		{
			this.context = context;
		}

		public void DrawEllipse (Rectangle frame, Pen pen = null, Brush brush = null)
		{
			if (pen == null && brush == null)
				return;
			var mode = SetPenAndBrush (pen, brush);
			var rect = Conversions.GetCGRect (frame);
			context.AddEllipseInRect (rect);
			context.DrawPath (mode);
		}

		CGPathDrawingMode SetPenAndBrush (Pen pen, Brush brush)
		{
			var mode = CGPathDrawingMode.Fill;
			if (brush != null) {
				SetBrush (brush);
				if (pen != null)
					mode = CGPathDrawingMode.FillStroke;
			}
			if (pen != null) {
				SetPen (pen);
				if (brush == null)
					mode = CGPathDrawingMode.Stroke;
			}
			return mode;
		}

		void SetPen (Pen pen)
		{
			context.SetStrokeColor ((nfloat)pen.Color.Red, (nfloat)pen.Color.Green, (nfloat)pen.Color.Blue, (nfloat)pen.Color.Alpha);
			context.SetLineWidth ((nfloat)pen.Width);
		}

		void SetBrush (Brush brush)
		{
			var sb = brush as SolidBrush;
			if (sb != null) {
				context.SetFillColor ((nfloat)sb.Color.Red, (nfloat)sb.Color.Green, (nfloat)sb.Color.Blue, (nfloat)sb.Color.Alpha);
			}
		}
	}

	public static class Conversions
	{
		public static CGRect GetCGRect (Rectangle frame)
		{
			return new CGRect ((nfloat)frame.X, (nfloat)frame.Y, (nfloat)frame.Width, (nfloat)frame.Height);
		}
	}
}

