using System;
using CoreGraphics;
using CoreText;
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
			context.TextMatrix = CGAffineTransform.MakeScale (1, -1);
		}

		public void SaveState ()
		{
			context.SaveState ();
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
					context.RotateCTM ((nfloat)(rt.Angle * Math.PI / 180));
					continue;
				}
				var tt = t as Translate;
				if (tt != null) {
					context.TranslateCTM ((nfloat)tt.Size.Width, (nfloat)tt.Size.Height);
					continue;
				}
				var st = t as Scale;
				if (st != null) {
					context.ScaleCTM ((nfloat)st.Size.Width, (nfloat)st.Size.Height);
					continue;
				}
				throw new NotSupportedException ("Transform " + t);
			}
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

		public void DrawText (string text, Rect frame, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null)
		{
			SetBrush (brush);

			context.SelectFont ("Georgia", 16, CGTextEncoding.MacRoman);
			context.ShowTextAtPoint ((nfloat)frame.X, (nfloat)frame.Y, text);


			using (var atext = new NSMutableAttributedString (text)) {

				atext.AddAttributes (new CTStringAttributes {
					ForegroundColor = new CGColor (1, 0, 0, 1),
				}, new NSRange (0, text.Length));

				using (var ct = new CTFramesetter (atext))
				using (var path = CGPath.FromRect (Conversions.GetCGRect (frame)))
				using (var tframe = ct.GetFrame (new NSRange (0, atext.Length), path, null))
					tframe.Draw (context);
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
				var start = Conversions.GetCGPoint (frame.Position + lgb.RelativeStart * size);
				var end = Conversions.GetCGPoint (frame.Position + lgb.RelativeEnd * size);
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
				var start = Conversions.GetCGPoint (frame.Position + rgb.RelativeCenter * size);
				var r = (nfloat)(rgb.RelativeRadius * size).Max;
				var end = Conversions.GetCGPoint (frame.Position + rgb.RelativeFocus * size);
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

				Rect bb = new Rect ();
				var nbb = 0;

				foreach (var op in ops) {
					var mt = op as MoveTo;
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
					var lt = op as LineTo;
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
					var ct = op as CurveTo;
					if (ct != null) {
						var p = ct.Point;
						var c1 = ct.Control1;
						var c2 = ct.Control2;
						context.AddCurveToPoint ((nfloat)c1.X, (nfloat)c1.Y, (nfloat)c2.X, (nfloat)c2.Y, (nfloat)p.X, (nfloat)p.Y);
						if (nbb == 0)
							bb = new Rect (p, Size.Zero);
						bb = bb.Union (p).Union (c1).Union (c2);
						nbb++;
						continue;
					}
					var cp = op as ClosePath;
					if (cp != null) {
						context.ClosePath ();
						continue;
					}

					throw new NotSupportedException ("Path Op " + op);
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

