using System;
using System.Linq;
using System.Reflection;

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
					#if MAC
					current = new ApplePlatform ();
					#elif __IOS__
					current = new ApplePlatform ();
					#elif __TVOS__
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
		public static void WriteSvg (this Graphic g, string path)
		{
#if NETFX_CORE
			throw new NotSupportedException ("Blame Microsoft for making a stupid decision to remove a critical API");
#else
			using (var w = new System.IO.StreamWriter (path, false, System.Text.Encoding.UTF8)) {
				g.WriteSvg (w);
			}
#endif
		}
	}
}

