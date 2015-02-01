using System;
using System.Linq;
using System.Reflection;

namespace NGraphics
{
	public interface IPlatform
	{
		string Name { get; }
		IImageCanvas CreateImageCanvas (Size size, double scale = 1.0, bool transparency = true);
		IImage CreateImage (Color[,] colors, double scale = 1.0);
	}

	public static class PlatformEx
	{
		public static IImage CreateImage (this IPlatform platform, Func<int, int, Color> colorFunc, Size size, double scale = 1.0)
		{
			var w = (int)Math.Ceiling (size.Width);
			var h = (int)Math.Ceiling (size.Height);
			var colors = new Color[w, h];
			for (var x = 0; x < w; x++) {
				for (var y = 0; y < h; y++) {
					colors [x, y] = colorFunc (x, y);
				}
			}
			return platform.CreateImage (colors, scale);
		}
	}
}

