using System;
using System.IO;
using System.Threading.Tasks;

namespace NGraphics
{
	public interface IPlatform
	{
		string Name { get; }
		IImageCanvas CreateImageCanvas (Size size, double scale = 1.0, bool transparency = true);
		IImage LoadImage (string path);
		IImage LoadImage (Stream stream);
		IImage CreateImage (Color[] colors, int pixelWidth, double scale = 1.0);
		TextMetrics MeasureText (string text, Font font);
		Task<Stream> OpenFileStreamForWritingAsync (string path);
	}

	public static class PlatformEx
	{
		public static GraphicCanvas CreateGraphicCanvas (this IPlatform platform, Size size)
		{
			return new GraphicCanvas (size, platform);
		}

		public static IImage CreateImage (this IPlatform platform, Func<int, int, Color> colorFunc, Size size, double scale = 1.0)
		{
			var w = (int)Math.Ceiling (size.Width);
			var h = (int)Math.Ceiling (size.Height);
			var colors = new Color[w * h];
			for (var y = 0; y < h; y++) {
				var o = y * w;
				for (var x = 0; x < w; x++) {
					colors [o + x] = colorFunc (x, y);
				}
			}
			return platform.CreateImage (colors, w, scale);
		}
	}
}

