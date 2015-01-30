using NUnit.Framework;
using System;
using NGraphics;
using System.Diagnostics;

namespace NGraphics.Test
{
	public class PlatformTest
	{
		public static IPlatform Platform = new NullPlatform ();
		public static string ResultsDirectory = "";
	}

	[TestFixture]
	public class ImageSurfaceTest : PlatformTest
	{
		[Test]
		public void Oval ()
		{
			var p = Platform;
			var s = p.CreateImageSurface (100, 100);
			s.DrawEllipse (new Point (10, 20), new Size (30, 40), Pens.Red.WithWidth (10), Brushes.Yellow);
			var i = s.GetImage ();
			var name = string.Format ("{0}-{1}.png", p.Name, "Oval");
			var path = System.IO.Path.Combine (ResultsDirectory, name);
			i.SaveAsPng (path);
			Debug.WriteLine (path);
//			System.Diagnostics.Process.Start ("open", "\"" + path + "\"");
		}
	}
}

