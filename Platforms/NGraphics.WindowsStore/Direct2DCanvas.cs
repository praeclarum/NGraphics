using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using SharpDX.Direct2D1;

namespace NGraphics.WindowsStore
{
	/// <summary>
	/// ICanvas wrapper over a Direct2D context.
	/// </summary>
	public class Direct2DCanvas : ICanvas
	{
        static readonly SharpDX.WIC.ImagingFactory _wicFactory;
        static readonly SharpDX.Direct2D1.Factory _d2DFactory;
        static readonly SharpDX.DirectWrite.Factory _dWriteFactory;
        static readonly SharpDX.Direct2D1.DeviceContext _d2DDeviceContext;

		static Direct2DCanvas ()
        {
            _wicFactory = new SharpDX.WIC.ImagingFactory ();
            _dWriteFactory = new SharpDX.DirectWrite.Factory ();

            var d3DDevice = new D3D11.Device(
                D3D.DriverType.Hardware,
				D3D11.DeviceCreationFlags.BgraSupport
#if DEBUG
				| D3D11.DeviceCreationFlags.Debug
#endif
				,
                D3D.FeatureLevel.Level_11_1,
				D3D.FeatureLevel.Level_11_0,
				D3D.FeatureLevel.Level_10_1,
				D3D.FeatureLevel.Level_10_0,
				D3D.FeatureLevel.Level_9_3,
				D3D.FeatureLevel.Level_9_2,
				D3D.FeatureLevel.Level_9_1
                );

            var dxgiDevice = ComObject.As<SharpDX.DXGI.Device> (d3DDevice.NativePointer);
            var d2DDevice = new Device(dxgiDevice);
            _d2DFactory = d2DDevice.Factory;
			_d2DDeviceContext = new DeviceContext (d2DDevice, DeviceContextOptions.None);
            _d2DDeviceContext.DotsPerInch = new Size2F (LogicalDpi, LogicalDpi);
        }

		static float LogicalDpi {
			get {
                return Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi;
			}
		}

		public void SaveState ()
		{
			throw new NotImplementedException ();
		}

		public void Transform (Transform transform)
		{
			throw new NotImplementedException ();
		}

		public void RestoreState ()
		{
			throw new NotImplementedException ();
		}

		public void DrawText (string text, Rect frame, Font font, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null)
		{
			throw new NotImplementedException ();
		}

		public void DrawPath (IEnumerable<PathOp> ops, Pen pen = null, Brush brush = null)
		{
			throw new NotImplementedException ();
		}

		public void DrawRectangle (Rect frame, Pen pen = null, Brush brush = null)
		{
			throw new NotImplementedException ();
		}

		public void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null)
		{
			throw new NotImplementedException ();
		}

		public void DrawImage (IImage image, Rect frame, double alpha = 1.0)
		{
			throw new NotImplementedException ();
		}
	}
}
