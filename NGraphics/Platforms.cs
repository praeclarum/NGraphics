using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NGraphics
{
	public static class Platforms
	{
		static readonly IPlatform nulll = new NullPlatform ();
		static IPlatform current = null;

		public static IPlatform Null { get { return nulll; } }

		public static IPlatform Current {
			get {
				if (current == null) {
#if __MACOS__ || __IOS__ || __MACCATALYST__ || __TVOS__ || __WATCHOS__
					current = new ApplePlatform ();
#elif __ANDROID__
					current = new AndroidPlatform ();
#elif NETFX_CORE
					current = new WinRTPlatform ();
#else
					current = new SystemDrawingPlatform ();
#endif
				}
				return current;
			}
		}
	}

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

