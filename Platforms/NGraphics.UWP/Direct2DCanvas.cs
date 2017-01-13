using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Mathematics.Interop;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using D2D1 = SharpDX.Direct2D1;
using DXGI = SharpDX.DXGI;
using WIC = SharpDX.WIC;
using DW = SharpDX.DirectWrite;

using Windows.UI.Xaml.Media.Imaging;
using NGraphics.UWP;
using System.Runtime.InteropServices;

namespace NGraphics
{
	public class WICBitmapCanvas : RenderTargetCanvas, IImageCanvas
	{
		protected readonly WIC.Bitmap Bmp;
		readonly Size size;
		readonly double scale;

		public WICBitmapCanvas (Size size, double scale = 1.0, bool transparency = true, Direct2DFactories factories = null)
			: this (
				// DIPs = pixels / (DPI/96.0)
				new WIC.Bitmap ((factories ?? Direct2DFactories.Shared).WICFactory, (int)(Math.Ceiling (size.Width * scale)), (int)(Math.Ceiling (size.Height * scale)), transparency ? WIC.PixelFormat.Format32bppPBGRA : WIC.PixelFormat.Format32bppBGR, WIC.BitmapCreateCacheOption.CacheOnLoad),
				new D2D1.RenderTargetProperties (D2D1.RenderTargetType.Default, new D2D1.PixelFormat (DXGI.Format.Unknown, D2D1.AlphaMode.Unknown), (float)(96.0 * scale), (float)(96.0 * scale), D2D1.RenderTargetUsage.None, D2D1.FeatureLevel.Level_DEFAULT))
		{
		}

		public WICBitmapCanvas (WIC.Bitmap bmp, D2D1.RenderTargetProperties properties, Direct2DFactories factories = null)
			: base (new D2D1.WicRenderTarget ((factories ?? Direct2DFactories.Shared).D2DFactory, bmp, properties), factories)
		{
			this.Bmp = bmp;
			this.scale = properties.DpiX / 96.0;
			var bmpSize = bmp.Size;
			this.size = new Size (bmpSize.Width / scale, bmpSize.Height / scale);
		}

		public IImage GetImage ()
		{
			renderTarget.EndDraw ();
			return new WICBitmapSourceImage (Bmp, scale, factories);
		}

		public Size Size
		{
			get { return size; }
		}

		public double Scale
		{
			get { return scale; }
		}
	}
    /// <summary>
    /// Provides access to an IMemoryBuffer as an array of bytes.
    /// </summary>
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public unsafe interface IMemoryBufferByteAccess
    {
        //If this method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.
        //When MemoryBuffer::Close is called, the code using this buffer should set the value pointer to null.
        /// <summary>
        /// Gets an IMemoryBuffer as an array of bytes. 
        /// </summary>
        /// <param name="buffer">A pointer to a byte array containing the buffer data.</param>
        /// <param name="capacity">The number of bytes in the returned array</param>
        void GetBuffer(out byte* buffer, out uint capacity);
    }
    public class WICBitmapSourceImage : IImage
	{
		readonly WIC.BitmapSource bmp;
		readonly Direct2DFactories factories;
		readonly double scale;

		public WIC.BitmapSource Bitmap { get { return bmp; } }

		public Size Size { get { return Conversions.ToSize (bmp.Size); } }
		public double Scale { get { return scale; } }

		public WICBitmapSourceImage (WIC.BitmapSource bmp, double scale, Direct2DFactories factories = null)
		{
			if (bmp == null)
				throw new ArgumentNullException ("bmp");
			this.bmp = bmp;
			this.scale = scale;
			this.factories = factories ?? Direct2DFactories.Shared;
		}

		public void SaveAsPng (string path)
		{
			throw new NotSupportedException ("WinUniversal does not support saving to files. Please use the Stream override instead.");
		}

		public void SaveAsPng (System.IO.Stream stream)
		{
			using (var encoder = new WIC.PngBitmapEncoder (factories.WICFactory, stream)) {
				using (var bitmapFrameEncode = new WIC.BitmapFrameEncode (encoder)) {
					bitmapFrameEncode.Initialize ();
					var size = bmp.Size;
					bitmapFrameEncode.SetSize (size.Width, size.Height);
					var pf = bmp.PixelFormat;
					bitmapFrameEncode.SetPixelFormat (ref pf);

					bitmapFrameEncode.WriteSource (bmp);

					bitmapFrameEncode.Commit ();
					encoder.Commit ();
				}
			}
		}
        /// <summary>
        /// Save As <see cref="Windows.Graphics.Imaging.SoftwareBitmap"/> 
        /// </summary>
        public unsafe Windows.Graphics.Imaging.SoftwareBitmap SaveAsSoftwareBitmap()
        {
            var size = bmp.Size;
            var sbitmap = new Windows.Graphics.Imaging.SoftwareBitmap(Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8, size.Width, size.Height,Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied);
            using (var sbitbuffer = sbitmap.LockBuffer(Windows.Graphics.Imaging.BitmapBufferAccessMode.Write))
            {
                using (var reference = sbitbuffer.CreateReference())
                {
                    byte* dataInBytes;
                    uint capacity;
                    ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacity);

                    // Fill-in the BGRA plane
                    var bufferLayout = sbitbuffer.GetPlaneDescription(0);
                    var intptr = new IntPtr(dataInBytes);
                    Bitmap.CopyPixels(bufferLayout.Stride, intptr, (int)capacity);
                }
            }
            return sbitmap;
        }
        /// <summary>
        /// Save As <see cref="SoftwareBitmapSource"/> 
        /// </summary>
        public SoftwareBitmapSource SaveAsSoftwareBitmapSource()
        {
            var source = new SoftwareBitmapSource();
            var task =source.SetBitmapAsync(SaveAsSoftwareBitmap());
            return source;
        }
        /// <summary>
        /// Save As <see cref="SoftwareBitmapSource"/> 
        /// </summary>
        public async Task<SoftwareBitmapSource> SaveAsSoftwareBitmapSourceAsync()
        {
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(SaveAsSoftwareBitmap());
            return source;
        }
    }

	public class SurfaceImageSourceCanvas : RenderTargetCanvas
	{
		DXGI.ISurfaceImageSourceNative sisn;

		public SurfaceImageSourceCanvas (SurfaceImageSource surfaceImageSource, Rect updateRect, Direct2DFactories factories = null)
			: base (factories)
		{
			sisn = ComObject.As<DXGI.ISurfaceImageSourceNative> (surfaceImageSource);
			RawPoint offset;
			var surface = sisn.BeginDraw (updateRect.ToRectangle (), out offset);

			var dpi = 96.0;
			var properties = new D2D1.RenderTargetProperties (D2D1.RenderTargetType.Default, new D2D1.PixelFormat (DXGI.Format.Unknown, D2D1.AlphaMode.Unknown), (float)(dpi), (float)(dpi), D2D1.RenderTargetUsage.None, D2D1.FeatureLevel.Level_DEFAULT);
			Initialize (new D2D1.RenderTarget (this.factories.D2DFactory, surface, properties));
		}
	}

	/// <summary>
	/// ICanvas wrapper over a Direct2D RenderTarget.
	/// </summary>
	public class RenderTargetCanvas : ICanvas
	{
		protected D2D1.RenderTarget renderTarget;
		protected readonly Direct2DFactories factories;
		readonly Stack<D2D1.DrawingStateBlock> stateStack = new Stack<D2D1.DrawingStateBlock> ();

		protected RenderTargetCanvas (Direct2DFactories factories = null)
		{
			this.factories = factories ?? Direct2DFactories.Shared;
		}

		public RenderTargetCanvas (DXGI.Surface surface, D2D1.RenderTargetProperties properties, Direct2DFactories factories = null)
		{
			if (surface == null)
				throw new ArgumentNullException ("surface");
			this.factories = factories ?? Direct2DFactories.Shared;
			Initialize (new D2D1.RenderTarget (this.factories.D2DFactory, surface, properties));
		}

		public RenderTargetCanvas (D2D1.RenderTarget renderTarget, Direct2DFactories factories = null)
		{
			if (renderTarget == null)
				throw new ArgumentNullException ("renderTarget");
			this.factories = factories ?? Direct2DFactories.Shared;
			Initialize (renderTarget);
		}

		protected void Initialize (D2D1.RenderTarget renderTarget)
		{
			this.renderTarget = renderTarget;
			renderTarget.BeginDraw ();
		}

		public void SaveState ()
		{
			var s = new D2D1.DrawingStateBlock (factories.D2DFactory);
			renderTarget.SaveDrawingState (s);
			stateStack.Push (s);
		}

		public void Transform (Transform transform)
		{
			var currentTx = renderTarget.Transform;
            var tx = new Matrix3x2 ();
            tx.M11 = (float)transform.A;
            tx.M12 = (float)transform.B;
            tx.M21 = (float)transform.C;
            tx.M22 = (float)transform.D;
            tx.M31 = (float)transform.E;
            tx.M32 = (float)transform.F;
			renderTarget.Transform = tx * currentTx;
		}

		public void RestoreState ()
		{
			if (stateStack.Count > 0) {
				var s = stateStack.Pop ();
				renderTarget.RestoreDrawingState (s);
			}
		}

		public TextMetrics MeasureText (string text, Font font)
		{
            return WinUniversalPlatform.GlobalMeasureText(factories, text, font);
		}

		public void DrawText (string text, Rect frame, Font font, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null)
		{
            try
            {
                if(brush==null)
                {
                    brush = new SolidBrush(Color.FromRGB(0, 0, 0, 0));
                }
                var layout = new DW.TextLayout(factories.DWFactory, text, WinUniversalPlatform.GetTextFormat(factories, font), (float)frame.Width, (float)frame.Height);
                var h = layout.Metrics.Height + layout.OverhangMetrics.Top;
                var point = (frame.TopLeft - h * Point.OneY).ToVector2();
                if (pen==null)
                {
                    renderTarget.DrawTextLayout(point, layout, GetBrush(frame, brush));
                }
                else
                {
                    using (var pentr = new TextRendererWithPen(factories.D2DFactory, renderTarget))
                    {
                        pentr.PenBrush = GetBrush(pen);
                        pentr.PenWidth = (float)pen.Width;
                        pentr.PenStyle = GetStrokeStyle(pen);
                        pentr.FontBrush = GetBrush(frame,brush);
                        layout.Draw(pentr, point.X, point.Y);
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("drawtexterror");
            }
		}

		public void DrawPath (IEnumerable<PathOp> ops, Pen pen = null, Brush brush = null)
		{
			var bb = new BoundingBoxBuilder ();
			var s = new D2D1.PathGeometry (factories.D2DFactory);
			var figureDepth = 0;
			using (var sink = s.Open ()) {
				foreach (var op in ops) {
					if (op is MoveTo) {
						while (figureDepth > 0) {
							sink.EndFigure (D2D1.FigureEnd.Open);
							figureDepth--;
						}
						var mt = ((MoveTo)op);
						sink.BeginFigure (Conversions.ToVector2 (mt.Point), D2D1.FigureBegin.Filled);
						figureDepth++;
						bb.Add (mt.Point);
					}
					else if (op is LineTo) {
						var lt = ((LineTo)op);
						sink.AddLine (Conversions.ToVector2 (lt.Point));
						bb.Add (lt.Point);
					}
					else if (op is ArcTo) {
						var ar = ((ArcTo)op);
						// TODO: Direct2D Arcs
						//sink.AddArc (new D2D1.ArcSegment {
						//	Size = Conversions.ToSize2F (ar.Radius),
						//	Point = Conversions.ToVector2 (ar.Point),
						//	SweepDirection = ar.SweepClockwise ? D2D1.SweepDirection.Clockwise : D2D1.SweepDirection.CounterClockwise,
						//});
						sink.AddLine (Conversions.ToVector2 (ar.Point));
						bb.Add (ar.Point);
					}
					else if (op is CurveTo) {
						var ct = ((CurveTo)op);
						sink.AddBezier (new D2D1.BezierSegment {
							Point1 = Conversions.ToVector2 (ct.Control1),
							Point2 = Conversions.ToVector2 (ct.Control2),
							Point3 = Conversions.ToVector2 (ct.Point),
						});
						bb.Add (ct.Point);
						bb.Add (ct.Control1);
						bb.Add (ct.Control2);
					}
					else if (op is ClosePath) {
						sink.EndFigure (D2D1.FigureEnd.Closed);
						figureDepth--;
					}
					else {
						// TODO: More path operations
					}
				}
				while (figureDepth > 0) {
					sink.EndFigure (D2D1.FigureEnd.Open);
					figureDepth--;
				}
				sink.Close ();
			}

			var p = GetBrush (pen);
			var b = GetBrush (bb.BoundingBox, brush);
				
			if (b != null) {
				renderTarget.FillGeometry (s, b);
			}
			if (p != null) {
				renderTarget.DrawGeometry (s, p, (float)pen.Width, GetStrokeStyle (pen));
			}			
		}

		public void DrawRectangle (Rect frame, Size corner, Pen pen = null, Brush brush = null)
		{
			var p = GetBrush (pen);
			var b = GetBrush (frame, brush);
			if (b != null) {
				if (corner.Width > 0 || corner.Height > 0) {
					var rr = new D2D1.RoundedRectangle ();
					rr.Rect = frame.ToRectangleF ();
					rr.RadiusX = (float)corner.Width;
					rr.RadiusY = (float)corner.Height;
					renderTarget.FillRoundedRectangle (ref rr, b);
				}
				else {
					renderTarget.FillRectangle (frame.ToRectangleF (), b);
				}
			}
			if (p != null) {
				if (corner.Width > 0 || corner.Height > 0) {
					var rr = new D2D1.RoundedRectangle ();
					rr.Rect = frame.ToRectangleF ();
					rr.RadiusX = (float)corner.Width;
					rr.RadiusY = (float)corner.Height;
					renderTarget.DrawRoundedRectangle (ref rr, p, (float)pen.Width, GetStrokeStyle (pen));
				}
				else {
					renderTarget.DrawRectangle (frame.ToRectangleF (), p, (float)pen.Width, GetStrokeStyle (pen));
				}
			}
		}

		public void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null)
		{
			var p = GetBrush (pen);
			var b = GetBrush (frame, brush);
			var c = frame.Center;
			var s = new D2D1.Ellipse (new RawVector2 { X = (float)c.X, Y = (float)c.Y }, (float)(frame.Width / 2.0), (float)(frame.Height / 2.0));
			if (b != null) {
				renderTarget.FillEllipse (s, b);
			}
			if (p != null) {
				renderTarget.DrawEllipse (s, p, (float)pen.Width, GetStrokeStyle (pen));
			}
		}

		public void DrawImage (IImage image, Rect frame, double alpha = 1.0)
		{
			var i = GetImage (image);
			renderTarget.DrawBitmap (i, frame.ToRectangleF (), (float)alpha, D2D1.BitmapInterpolationMode.Linear);
		}

		D2D1.Bitmap GetImage (IImage image)
		{
			if (image == null)
				return null;

			var wbi = image as WICBitmapSourceImage;
			if (wbi != null) {
				Guid renderFormat = WIC.PixelFormat.Format32bppPBGRA;
				if (wbi.Bitmap.PixelFormat != renderFormat) {
					//System.Diagnostics.Debug.WriteLine ("RT  FORMAT: " + renderTarget.PixelFormat.Format);
					//System.Diagnostics.Debug.WriteLine ("BMP FORMAT: " + wbi.Bitmap.PixelFormat);
					var c = new WIC.FormatConverter (factories.WICFactory);
					c.Initialize (wbi.Bitmap, renderFormat);
					//System.Diagnostics.Debug.WriteLine ("CO  FORMAT: " + c.PixelFormat);
					return D2D1.Bitmap.FromWicBitmap (renderTarget, c);
				}
				else {
					return D2D1.Bitmap.FromWicBitmap (renderTarget, wbi.Bitmap);
				}
			}

			throw new NotSupportedException ("Image type " + image.GetType () + " not supported");
		}

		D2D1.StrokeStyle GetStrokeStyle (Pen pen)
		{
			return null;
		}

		D2D1.Brush GetBrush (Pen pen)
		{
			if (pen == null) return null;
			return new D2D1.SolidColorBrush (renderTarget, pen.Color.ToColor4 ());
		}

		D2D1.Brush GetBrush (Rect frame, Brush brush)
		{
			if (brush == null) return null;
			var sb = brush as SolidBrush;
			if (sb != null) {
				return new D2D1.SolidColorBrush (renderTarget, sb.Color.ToColor4 ());
			}

			var lgb = brush as LinearGradientBrush;
			if (lgb != null) {
				if (lgb.Stops.Count < 2) return null;
				var props = new D2D1.LinearGradientBrushProperties {
					StartPoint = lgb.GetAbsoluteStart (frame).ToVector2 (),
					EndPoint = lgb.GetAbsoluteEnd (frame).ToVector2 (),
				};
				return new D2D1.LinearGradientBrush (renderTarget, props, GetStops (lgb.Stops));
			}

			var rgb = brush as RadialGradientBrush;
			if (rgb != null) {
				if (rgb.Stops.Count < 2) return null;
				var rad = rgb.GetAbsoluteRadius (frame);
				var center = rgb.GetAbsoluteCenter (frame);
				var focus = rgb.GetAbsoluteFocus (frame);
				var props = new D2D1.RadialGradientBrushProperties {
					Center = center.ToVector2 (),
					RadiusX = (float)rad.Width,
					RadiusY = (float)rad.Height,
					GradientOriginOffset = (focus - center).ToVector2 (),
				};
				return new D2D1.RadialGradientBrush (renderTarget, props, GetStops (rgb.Stops));
			}

			// TODO: Radial gradient brushes
			return new D2D1.SolidColorBrush (renderTarget, Colors.Black.ToColor4 ());
		}

		D2D1.GradientStopCollection GetStops (List<GradientStop> stops)
		{
			var q =
				stops.
				Select (s => new D2D1.GradientStop {
					Color = s.Color.ToColor4 (),
					Position = (float)s.Offset,
				});
			return new D2D1.GradientStopCollection (renderTarget, q.ToArray ());
		}
	}

	public class Direct2DFactories
	{
		public readonly WIC.ImagingFactory WICFactory;
		public readonly D2D1.Factory D2DFactory;
		public readonly DW.Factory DWFactory;
		//public readonly D2D1.DeviceContext _d2DDeviceContext;

		public static readonly Direct2DFactories Shared = new Direct2DFactories ();

		public Direct2DFactories ()
		{
			WICFactory = new WIC.ImagingFactory ();
			DWFactory = new DW.Factory ();

			var d3DDevice = new D3D11.Device (
				D3D.DriverType.Hardware,
				D3D11.DeviceCreationFlags.BgraSupport,
				D3D.FeatureLevel.Level_11_1,
				D3D.FeatureLevel.Level_11_0,
				D3D.FeatureLevel.Level_10_1,
				D3D.FeatureLevel.Level_10_0,
				D3D.FeatureLevel.Level_9_3,
				D3D.FeatureLevel.Level_9_2,
				D3D.FeatureLevel.Level_9_1
				);

			var dxgiDevice = ComObject.As<SharpDX.DXGI.Device> (d3DDevice.NativePointer);
			var d2DDevice = new D2D1.Device (dxgiDevice);
			D2DFactory = d2DDevice.Factory;
			//_d2DDeviceContext = new D2D1.DeviceContext (d2DDevice, D2D1.DeviceContextOptions.None);
			//var dpi = DisplayDpi;
			//_d2DDeviceContext.DotsPerInch = new Size2F (dpi, dpi);
		}

		/*static float DisplayDpi
		{
			get
			{
				return Windows.Graphics.Display.DisplayInformation.GetForCurrentView ().LogicalDpi;
			}
		}*/
	}

	public static partial class Conversions
	{
		public static RawColor4 ToColor4 (this Color color)
		{
			return new RawColor4 ((float)color.Red, (float)color.Green, (float)color.Blue, (float)color.Alpha);
		}

		public static RawVector2 ToVector2 (this Point point)
		{
			return new RawVector2 { X = (float)point.X, Y = (float)point.Y };
		}

		public static Size2F ToSize2F (this Size size)
		{
			return new Size2F ((float)size.Width, (float)size.Height);
		}

		public static Size ToSize (this Size2F size)
		{
			return new Size (size.Width, size.Height);
		}

		public static Size ToSize (this Size2 size)
		{
			return new Size (size.Width, size.Height);
		}

		public static RawRectangleF ToRectangleF (this Rect rect)
		{
			return new RawRectangleF ((float)rect.X, (float)rect.Y, (float)rect.Right, (float)rect.Bottom);
		}

		public static RawRectangle ToRectangle (this Rect rect)
		{
			return new RawRectangle ((int)rect.X, (int)rect.Y, (int)rect.Right, (int)rect.Bottom);
		}
	}
}
