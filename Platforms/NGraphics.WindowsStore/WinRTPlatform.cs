using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using D2D1 = SharpDX.Direct2D1;
using DXGI = SharpDX.DXGI;
using DW = SharpDX.DirectWrite;
using WIC = SharpDX.WIC;

namespace NGraphics
{
	public class WinRTPlatform : IPlatform
	{
		public string Name { get { return "WinRT"; } }

		public IImageCanvas CreateImageCanvas (Size size, double scale = 1.0, bool transparency = true)
		{
			return new WICBitmapCanvas (size, scale, transparency);
		}

		public IImage CreateImage (Color[] colors, int width, double scale = 1.0)
		{
			var factories = Direct2DFactories.Shared;
			var pf = WIC.PixelFormat.Format32bppBGRA;

			unsafe {
				fixed (Color* p = colors) {
					var data = new DataRectangle {
						Pitch = width * 4,
						DataPointer = (IntPtr)p,
					};
					var bmp = new WIC.Bitmap (factories.WICFactory, width, colors.Length / width, pf, data);
					return new WICBitmapSourceImage (bmp, scale, factories);
				}
			}
		}

		public IImage LoadImage (Stream stream)
		{
			var factories = Direct2DFactories.Shared;
			var d = new WIC.BitmapDecoder (factories.WICFactory, stream, WIC.DecodeOptions.CacheOnDemand);
			WIC.BitmapSource b = d.GetFrame (0);

			var renderFormat = WIC.PixelFormat.Format32bppPBGRA;
			if (b.PixelFormat != renderFormat) {
				//System.Diagnostics.Debug.WriteLine ("BMP FORMAT: " + b.PixelFormat);
				var c = new WIC.FormatConverter (factories.WICFactory);
				c.Initialize (b, renderFormat);
				//System.Diagnostics.Debug.WriteLine ("CO  FORMAT: " + c.PixelFormat);
				b = c;				
			}

			// Convert the BitmapSource to a Bitmap so we can allow the decoder to go out of memory
			return new WICBitmapSourceImage (new WIC.Bitmap (factories.WICFactory, b, WIC.BitmapCreateCacheOption.CacheOnLoad), 1.0, factories);
		}

		public IImage LoadImage (string path)
		{
			throw new NotSupportedException ("WinRT cannot load images from file paths. Use the Stream overload instead.");
		}

		public async Task<Stream> OpenFileStreamForWritingAsync (string path)
		{
			var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync (System.IO.Path.GetDirectoryName (path));
			return await folder.OpenStreamForWriteAsync (System.IO.Path.GetFileName (path), Windows.Storage.CreationCollisionOption.ReplaceExisting);
		}

        public static DW.TextFormat GetTextFormat (Direct2DFactories factories, Font font)
        {
            return new DW.TextFormat (factories.DWFactory, font.Family, (float)font.Size);
        }

        public static TextMetrics GlobalMeasureText (Direct2DFactories factories, string text, Font font)
        {
            float maxWidth = float.MaxValue;
            float maxHeight = float.MaxValue;
            var layout = new DW.TextLayout(factories.DWFactory, text, GetTextFormat(factories, font), maxWidth, maxHeight);
            return new TextMetrics
            {
                Width = layout.Metrics.Width,
                Ascent = layout.Metrics.Height + layout.OverhangMetrics.Top,
                Descent = -layout.OverhangMetrics.Top,
            };
        }

        public TextMetrics MeasureText (string text, Font font)
        {
            return GlobalMeasureText (Direct2DFactories.Shared, text, font);
        }
    }
}
