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

		public static IPlatform Null => nulll;

        public static void SetPlatform(IPlatform platform) => current = platform;

        public static void SetPlatform<TPlatform>() where TPlatform : IPlatform, new()  => current = new TPlatform();

        public static IPlatform Current { 
			get {
				if (current == null) {
					throw new Exception("Platform must be first set using " + nameof(SetPlatform));
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

