using NUnit.Framework;
using System.IO;
using System;
using System.Reflection;

namespace NGraphics.Test
{
	[TestFixture]
	public class SvgReaderTests : PlatformTest
	{
		Stream OpenResource (string path)
		{
			var assembly = typeof (SvgReaderTests).GetTypeInfo ().Assembly;
			var resources = assembly.GetManifestResourceNames ();
			return assembly.GetManifestResourceStream ("NGraphics.Test.Inputs." + path);
		}

		Graphic Read (string path)
		{
			using (var s = OpenResource (path)) {
				var r = new SvgReader (new StreamReader (s));
				Assert.GreaterOrEqual (r.Graphic.Children.Count, 0);
				return r.Graphic;
			}
		}

		void ReadAndDraw (string path)
		{
			var g = Read (path);
			var c = Platform.CreateImageSurface ((int)Math.Ceiling (g.Size.Width), (int)Math.Ceiling (g.Size.Height), true);
			g.Draw (c);
			c.GetImage ().SaveAsPng (ResultPath (path));
		}

		[Test]
		public void MozillaEllipse ()
		{
			ReadAndDraw ("mozilla.ellipse.svg");
		}

		[Test]
		public void SunAtNight ()
		{
			ReadAndDraw ("SunAtNight.svg");
		}
	}
}

