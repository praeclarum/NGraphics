using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NGraphics.UWP
{
	public static class GraphicFilesEx
	{
		public static async Task WriteSvgAsync (this Graphic g, string path)
		{
			using (var s = await Platforms.Current.OpenFileStreamForWritingAsync (path)) {
				using (var w = new System.IO.StreamWriter (s, System.Text.Encoding.UTF8)) {
					g.WriteSvg (w);
				}
			}
		}
	}
}

