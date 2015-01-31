using System;
using CoreGraphics;
using ImageIO;
using Foundation;
using System.Linq;
using System.Collections.Generic;

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
			var bitmapInfo = transparency ? CGImageAlphaInfo.PremultipliedFirst : CGImageAlphaInfo.None;
			var bitsPerComp = 8;
			var bytesPerRow = transparency ? 4 * pixelWidth : 3 * pixelWidth;
			var colorSpace = CGColorSpace.CreateDeviceRGB ();
			var bitmap = new CGBitmapContext (IntPtr.Zero, pixelWidth, pixelHeight, bitsPerComp, bytesPerRow, colorSpace, bitmapInfo);
			return new CGBitmapContextCanvas (bitmap, scale);
		}
	}

	public class CGBitmapContextCanvas : CGContextCanvas, IImageCanvas
	{
		CGBitmapContext context;
		readonly double scale;

		public CGBitmapContextCanvas (CGBitmapContext context, double scale)
			: base (context)
		{
			this.context = context;
			this.scale = scale;

			this.context.TranslateCTM (0, context.Height);
			this.context.ScaleCTM (1, -1);
		}

		public IImage GetImage ()
		{
			return new CGImageImage (this.context.ToImage (), scale);
		}
	}

	public class CGImageImage : IImage
	{
		CGImage image;
		readonly double scale;

		public CGImageImage (CGImage image, double scale)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			this.image = image;
			this.scale = scale;
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
	}

	public class CGContextCanvas : ICanvas
	{
		readonly CGContext context;

		public CGContextCanvas (CGContext context)
		{
			this.context = context;
		}

		public void SaveState ()
		{
			context.SaveState ();
		}
		public void Transform (Transform transform)
		{
			var t = transform;
			while (t != null) {
				var rt = t as Rotate;
				if (rt != null) {
					context.RotateCTM ((nfloat)rt.Angle);
					t = t.Previous;
					continue;
				}
				var tt = t as Translate;
				if (tt != null) {
					context.TranslateCTM ((nfloat)tt.Size.Width, (nfloat)tt.Size.Height);
					t = t.Previous;
					continue;
				}
				throw new NotSupportedException ("Transform " + t);
			}
		}
		public void RestoreState ()
		{
			context.RestoreState ();
		}

		CGGradient CreateGradient (LinearGradientBrush brush)
		{
			var n = brush.Stops.Count;
			var locs = new nfloat [n];
			var comps = new nfloat [4 * n];
			for (var i = 0; i < n; i++) {
				var s = brush.Stops [i];
				locs [i] = (nfloat)s.Offset;
				comps [4 * i + 0] = (nfloat)s.Color.Red;
				comps [4 * i + 1] = (nfloat)s.Color.Green;
				comps [4 * i + 2] = (nfloat)s.Color.Blue;
				comps [4 * i + 3] = (nfloat)s.Color.Alpha;
			}
			var cs = CGColorSpace.CreateDeviceRGB ();
			return new CGGradient (cs, comps, locs);
		}

		void DrawElement (Func<Rect> add, Pen pen = null, Brush brush = null)
		{
			if (pen == null && brush == null)
				return;

			var lgb = brush as LinearGradientBrush;
			if (lgb != null) {

				var cg = CreateGradient (lgb);
				context.SaveState ();
				var frame = add ();
				context.Clip ();
				CGGradientDrawingOptions options = CGGradientDrawingOptions.DrawsBeforeStartLocation | CGGradientDrawingOptions.DrawsAfterEndLocation;
				var size = frame.Size;
				var start = Conversions.GetCGPoint (frame.Position + lgb.RelativeStart * size);
				var end = Conversions.GetCGPoint (frame.Position + lgb.RelativeEnd * size);
				context.DrawLinearGradient (cg, start, end, options);
				context.RestoreState ();

				if (pen != null) {
					SetPen (pen);
					add ();
					context.StrokePath ();
				}

			} else {
				var mode = SetPenAndBrush (pen, brush);

				add ();
				context.DrawPath (mode);
			}
		}

		public void DrawPath (IEnumerable<PathCommand> commands, Pen pen = null, Brush brush = null)
		{
			if (pen == null && brush == null)
				return;

			DrawElement (() => {

				Rect bb = new Rect ();
				var nbb = 0;

				foreach (var c in commands) {
					var mt = c as MoveTo;
					if (mt != null) {
						var p = mt.Point;
						context.MoveTo ((nfloat)p.X, (nfloat)p.Y);
						if (nbb == 0)
							bb = new Rect (p, Size.Zero);
						else
							bb = bb.Union (p);
						nbb++;
						continue;
					}
					var lt = c as LineTo;
					if (lt != null) {
						var p = lt.Point;
						context.AddLineToPoint ((nfloat)p.X, (nfloat)p.Y);
						if (nbb == 0)
							bb = new Rect (p, Size.Zero);
						else
							bb = bb.Union (p);
						nbb++;
						continue;
					}
					var cp = c as ClosePath;
					if (cp != null) {
						context.ClosePath ();
						continue;
					}
				}

				return bb;

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
		public static CGPoint GetCGPoint (Point point)
		{
			return new CGPoint ((nfloat)point.X, (nfloat)point.Y);
		}

		public static CGRect GetCGRect (Rect frame)
		{
			return new CGRect ((nfloat)frame.X, (nfloat)frame.Y, (nfloat)frame.Width, (nfloat)frame.Height);
		}
	}
}

