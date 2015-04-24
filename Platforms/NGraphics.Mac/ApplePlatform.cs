using System;
using CoreGraphics;
using CoreText;
using ImageIO;
using Foundation;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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

		public IImageCanvas CreateImageCanvas (Size size, double scale = 1.0, bool transparency = true)
		{
			var pixelWidth = (int)Math.Ceiling (size.Width * scale);
			var pixelHeight = (int)Math.Ceiling (size.Height * scale);
			var bitmapInfo = transparency ? CGImageAlphaInfo.PremultipliedFirst : CGImageAlphaInfo.NoneSkipFirst;
			var bitsPerComp = 8;
			var bytesPerRow = transparency ? 4 * pixelWidth : 4 * pixelWidth;
			var colorSpace = CGColorSpace.CreateDeviceRGB ();
			var bitmap = new CGBitmapContext (IntPtr.Zero, pixelWidth, pixelHeight, bitsPerComp, bytesPerRow, colorSpace, bitmapInfo);
			return new CGBitmapContextCanvas (bitmap, scale);
		}

		public IImage CreateImage (Color[] colors, int width, double scale = 1.0)
		{
			var pixelWidth = width;
			var pixelHeight = colors.Length / width;
			var bitmapInfo = CGImageAlphaInfo.PremultipliedFirst;
			var bitsPerComp = 8;
			var bytesPerRow = width * 4;// ((4 * pixelWidth + 3)/4) * 4;
			var colorSpace = CGColorSpace.CreateDeviceRGB ();
			var bitmap = new CGBitmapContext (IntPtr.Zero, pixelWidth, pixelHeight, bitsPerComp, bytesPerRow, colorSpace, bitmapInfo);
			var data = bitmap.Data;
			unsafe {
				fixed (Color *c = colors) {					
					for (var y = 0; y < pixelHeight; y++) {
						var s = (byte*)c + 4*pixelWidth*y;
						var d = (byte*)data + bytesPerRow*y;
						for (var x = 0; x < pixelWidth; x++) {
							var b = *s++;
							var g = *s++;
							var r = *s++;
							var a = *s++;
							*d++ = a;
							*d++ = (byte)((r * a) >> 8);
							*d++ = (byte)((g * a) >> 8);
							*d++ = (byte)((b * a) >> 8);
						}
					}
				}
			}
			var image = bitmap.ToImage (); 
			return new CGImageImage (image, scale);
		}
		public IImage LoadImage (Stream stream)
		{
			var mem = new MemoryStream ((int)stream.Length);
			stream.CopyTo (mem);
			unsafe {
				fixed (byte *x = mem.GetBuffer ()) {
					var provider = new CGDataProvider (new IntPtr (x), (int)mem.Length, false);
					var image = CGImage.FromPNG (provider, null, false, CGColorRenderingIntent.Default);
					return new CGImageImage (image, 1);
				}
			}
		}
		public IImage LoadImage (string path)
		{
			var provider = new CGDataProvider (path);
			CGImage image;
			if (System.IO.Path.GetExtension (path).ToLowerInvariant () == ".png") {				
				image = CGImage.FromPNG (provider, null, false, CGColorRenderingIntent.Default);
			} else {
				image = CGImage.FromJPEG (provider, null, false, CGColorRenderingIntent.Default);
			}
			return new CGImageImage (image, 1);
		}
	}

	public class CGBitmapContextCanvas : CGContextCanvas, IImageCanvas
	{
		CGBitmapContext context;
		readonly double scale;

		public Size Size { get { return new Size (context.Width / scale, context.Height / scale); } }
		public Size SizeInPixels { get { return new Size (context.Width, context.Height); } }
		public double Scale { get { return scale; } }

		public CGBitmapContextCanvas (CGBitmapContext context, double scale)
			: base (context)
		{
			this.context = context;
			this.scale = scale;

			var nscale = (nfloat)scale;
			this.context.TranslateCTM (0, context.Height);
			this.context.ScaleCTM (nscale, -nscale);
		}

		public IImage GetImage ()
		{
			return new CGImageImage (this.context.ToImage (), scale);
		}
	}

	public class CGImageImage : IImage
	{
		readonly CGImage image;
		readonly double scale;
		readonly Size size;

		public CGImage Image { get { return image; } }
		public double Scale { get { return scale; } }
		public Size Size { get { return size; } }

		public CGImageImage (CGImage image, double scale)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			this.image = image;
			this.scale = scale;
			this.size = new Size (image.Width / scale, image.Height / scale);
		}

		public void SaveAsPng (string path)
		{
			if (string.IsNullOrEmpty (path))
				throw new ArgumentException ("path");
			using (var dest = CGImageDestination.Create (NSUrl.FromFilename (path), "public.png", 1)) {
				if (dest == null) {
					throw new InvalidOperationException (string.Format ("Could not create image destination {0}.", path));
				}
				dest.AddImage (image);
				dest.Close ();
			}
		}

		public void SaveAsPng (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ();
			
			using (var data = new NSMutableData ()) {
				using (var dest = CGImageDestination.Create (data, "public.png", 1)) {
					if (dest == null) {
						throw new InvalidOperationException (string.Format ("Could not create image destination from {0}.", stream));
					}
					dest.AddImage (image);
					dest.Close ();
				}
				data.AsStream ().CopyTo (stream);
			}
		}
	}

	public class CGContextCanvas : ICanvas
	{
		readonly CGContext context;

		public CGContext Context { get { return context; } }

		public CGContextCanvas (CGContext context)
		{
			this.context = context;
//			context.InterpolationQuality = CGInterpolationQuality.High;
			context.TextMatrix = CGAffineTransform.MakeScale (1, -1);
		}

		public void SaveState ()
		{
			context.SaveState ();
		}
		public void Transform (Transform transform)
		{
			context.ConcatCTM (new CGAffineTransform (
				(nfloat)transform.A, (nfloat)transform.B,
				(nfloat)transform.C, (nfloat)transform.D,
				(nfloat)transform.E, (nfloat)transform.F));
		}
		public void RestoreState ()
		{
			context.RestoreState ();
		}

		CGGradient CreateGradient (IList<GradientStop> stops)
		{
			var n = stops.Count;
			var locs = new nfloat [n];
			var comps = new nfloat [4 * n];
			for (var i = 0; i < n; i++) {
				var s = stops [i];
				locs [i] = (nfloat)s.Offset;
				comps [4 * i + 0] = (nfloat)s.Color.Red;
				comps [4 * i + 1] = (nfloat)s.Color.Green;
				comps [4 * i + 2] = (nfloat)s.Color.Blue;
				comps [4 * i + 3] = (nfloat)s.Color.Alpha;
			}
			var cs = CGColorSpace.CreateDeviceRGB ();
			return new CGGradient (cs, comps, locs);
		}

		private static NSString NSFontAttributeName = new NSString("NSFontAttributeName");

		public Size MeasureText(string text, Font font)
		{
			if (string.IsNullOrEmpty(text))
				return Size.Zero;
			if (font == null)
				throw new ArgumentNullException("font");

			string fontName = font.Name;
			Array availableFonts =
				#if __IOS__
				UIKit.UIFont.FontNamesForFamilyName(fontName);
				#else
				AppKit.NSFontManager.SharedFontManager.AvailableMembersOfFontFamily (fontName).ToArray ();
				#endif

			#if __IOS__
			UIKit.UIFont nsFont;
			if (availableFonts != null && availableFonts.Length > 0)
				nsFont = UIKit.UIFont.FromName(font.Name, (nfloat)font.Size);
			else
				nsFont = UIKit.UIFont.FromName("Georgia", (nfloat)font.Size);
			#else
			AppKit.NSFont nsFont;
			if (availableFonts != null && availableFonts.Length > 0)
				nsFont = AppKit.NSFont.FromFontName(font.Name, (nfloat)font.Size);
			else
				nsFont = AppKit.NSFont.FromFontName("Georgia", (nfloat)font.Size);
			#endif

			using (var s = new NSAttributedString(text, font: nsFont))
			using (nsFont)
			{
				#if __IOS__
				var result = s.GetBoundingRect(new CGSize(float.MaxValue, float.MaxValue), NSStringDrawingOptions.UsesDeviceMetrics, null);
				#else
				var result = s.BoundingRectWithSize(new CGSize(float.MaxValue, float.MaxValue), NSStringDrawingOptions.UsesDeviceMetrics);
				#endif
				return new Size(result.Width, result.Height);
			}
		}

		public void DrawText (string text, Rect frame, Font font, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null)
		{
			if (string.IsNullOrEmpty (text))
				return;
			if (font == null)
				throw new ArgumentNullException ("font");

			SetBrush (brush);

//			string fontName = font.Name;
//			context.SelectFont (font.Name, (nfloat)font.Size, CGTextEncoding.MacRoman);
//			
//			context.ShowTextAtPoint ((nfloat)frame.X, (nfloat)frame.Y, text);

			var pt = frame.TopLeft;

			using (var atext = new NSMutableAttributedString (text)) {

				atext.AddAttributes (new CTStringAttributes {
					ForegroundColorFromContext = true,
					Font = font.GetCTFont (),
				}, new NSRange (0, text.Length));

				using (var l = new CTLine (atext)) {
					nfloat asc, desc, lead;
					var width = l.GetTypographicBounds (out asc, out desc, out lead);
					context.SaveState ();
					context.TranslateCTM ((nfloat)(pt.X - width / 2), (nfloat)(pt.Y + desc));
					context.TextPosition = CGPoint.Empty;
					l.Draw (context);
					context.RestoreState ();
				}
			}
		}

		void DrawElement (Func<Rect> add, Pen pen = null, Brush brush = null)
		{
			if (pen == null && brush == null)
				return;

			var lgb = brush as LinearGradientBrush;
			if (lgb != null) {
				var cg = CreateGradient (lgb.Stops);
				context.SaveState ();
				var frame = add ();
				context.Clip ();
				CGGradientDrawingOptions options = CGGradientDrawingOptions.DrawsBeforeStartLocation | CGGradientDrawingOptions.DrawsAfterEndLocation;
				var size = frame.Size;
				var start = Conversions.GetCGPoint (lgb.Absolute ? lgb.Start : frame.Position + lgb.Start * size);
				var end = Conversions.GetCGPoint (lgb.Absolute ? lgb.End : frame.Position + lgb.End * size);
				context.DrawLinearGradient (cg, start, end, options);
				context.RestoreState ();
				brush = null;
			}

			var rgb = brush as RadialGradientBrush;
			if (rgb != null) {
				var cg = CreateGradient (rgb.Stops);
				context.SaveState ();
				var frame = add ();
				context.Clip ();
				CGGradientDrawingOptions options = CGGradientDrawingOptions.DrawsBeforeStartLocation | CGGradientDrawingOptions.DrawsAfterEndLocation;
				var size = frame.Size;
				var start = Conversions.GetCGPoint (rgb.GetAbsoluteCenter (frame));
				var r = (nfloat)rgb.GetAbsoluteRadius (frame).Max;
				var end = Conversions.GetCGPoint (rgb.GetAbsoluteFocus (frame));
				context.DrawRadialGradient (cg, start, 0, end, r, options);
				context.RestoreState ();
				brush = null;
			}

			if (pen != null || brush != null)
			{
				var mode = SetPenAndBrush (pen, brush);

				add ();
				context.DrawPath (mode);
			}
		}

		public void DrawPath (IEnumerable<PathOp> ops, Pen pen = null, Brush brush = null)
		{
			if (pen == null && brush == null)
				return;

			DrawElement (() => {

				var bb = new BoundingBoxBuilder ();

				foreach (var op in ops) {
					var mt = op as MoveTo;
					if (mt != null) {
						var p = mt.Point;
						context.MoveTo ((nfloat)p.X, (nfloat)p.Y);
						bb.Add (p);
						continue;
					}
					var lt = op as LineTo;
					if (lt != null) {
						var p = lt.Point;
						context.AddLineToPoint ((nfloat)p.X, (nfloat)p.Y);
						bb.Add (p);
						continue;
					}
					var at = op as ArcTo;
					if (at != null) {
						var p = at.Point;
						var pp = Conversions.GetPoint (context.GetPathCurrentPoint ());
						Point c1, c2;
						at.GetCircles (pp, out c1, out c2);
						context.AddLineToPoint ((nfloat)p.X, (nfloat)p.Y);
						bb.Add (p);
						continue;
					}
					var ct = op as CurveTo;
					if (ct != null) {
						var p = ct.Point;
						var c1 = ct.Control1;
						var c2 = ct.Control2;
						context.AddCurveToPoint ((nfloat)c1.X, (nfloat)c1.Y, (nfloat)c2.X, (nfloat)c2.Y, (nfloat)p.X, (nfloat)p.Y);
						bb.Add (p);
						bb.Add (c1);
						bb.Add (c2);
						continue;
					}
					var cp = op as ClosePath;
					if (cp != null) {
						context.ClosePath ();
						continue;
					}

					throw new NotSupportedException ("Path Op " + op);
				}

				return bb.BoundingBox;

			}, pen, brush);
		}
		public void DrawRectangle (Rect frame, Pen pen = null, Brush brush = null)
		{
			if (pen == null && brush == null)
				return;

			DrawElement (() => {
				context.AddRect (Conversions.GetCGRect (frame));
				return frame;
			}, pen, brush);
		}
		public void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null)
		{
			if (pen == null && brush == null)
				return;

			DrawElement (() => {
				context.AddEllipseInRect (Conversions.GetCGRect (frame));
				return frame;
			}, pen, brush);
		}
		public void DrawImage (IImage image, Rect frame, double alpha = 1.0)
		{
			var cgi = image as CGImageImage;

			if (cgi != null) {
				var i = cgi.Image;
				var h = frame.Height;
				context.SaveState ();
				context.SetAlpha ((nfloat)alpha);
				context.TranslateCTM ((nfloat)frame.X, (nfloat)(h + frame.Y));
				context.ScaleCTM (1, -1);
				context.DrawImage (new CGRect (0, 0, (nfloat)frame.Width, (nfloat)frame.Height), cgi.Image);
				context.RestoreState ();
			}
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
		public static CGPoint GetCGPoint (this Point point)
		{
			return new CGPoint ((nfloat)point.X, (nfloat)point.Y);
		}
		public static Point GetPoint (this CGPoint point)
		{
			return new Point (point.X, point.Y);
		}
		public static Point ToPoint (this CGPoint point)
		{
			return new Point (point.X, point.Y);
		}
		public static Size GetSize (this CGSize size)
		{
			return new Size (size.Width, size.Height);
		}
		public static CGSize GetCGSize (this Size size)
		{
			return new CGSize ((nfloat)size.Width, (nfloat)size.Height);
		}
		public static CGRect GetCGRect (this Rect frame)
		{
			return new CGRect ((nfloat)frame.X, (nfloat)frame.Y, (nfloat)frame.Width, (nfloat)frame.Height);
		}
		public static Rect GetRect (this CGRect rect)
		{
			return new Rect (rect.X, rect.Y, rect.Width, rect.Height);
		}
		public static CTFont GetCTFont (this Font font)
		{
			return new CTFont (font.Name, (nfloat)font.Size);
		}
		#if __IOS__
		public static UIKit.UIImage GetUIImage (this IImage image)
		{
			var c = (CGImageImage)image;
			return new UIKit.UIImage (c.Image, (nfloat)c.Scale, UIKit.UIImageOrientation.Up);
		}
		#else
		public static AppKit.NSImage GetNSImage (this IImage image)
		{
			var c = (CGImageImage)image;
			return new AppKit.NSImage (c.Image, Conversions.GetCGSize (c.Size));
		}
		#endif
	}
}

