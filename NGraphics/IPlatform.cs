using System;
using System.Linq;

namespace NGraphics
{
	public interface IPlatform
	{
		IImageSurface CreateImageSurface (int pixelWidth, int pixelHeight, bool transparency = true);
	}

	public static class Platforms
	{
		static readonly IPlatform nulll = new NullPlatform ();
		static IPlatform current = null;

		public static IPlatform Null { get { return nulll; } }

		public static IPlatform Current {
			get {
				if (current == null) {
					try {
						var types = System.Reflection.Assembly.GetCallingAssembly ().GetTypes ();
						var ipt = typeof(IPlatform);
						var npt = typeof(NullPlatform);
						var pt = types.FirstOrDefault (t => !t.IsAbstract && ipt.IsAssignableFrom (t) && !npt.IsAssignableFrom (t));
						current = (IPlatform)Activator.CreateInstance (pt);
					} catch (Exception ex) {
						current = nulll;
					}
				}
				return current;
			}
		}
	}
}

