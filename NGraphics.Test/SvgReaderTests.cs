using NUnit.Framework;
using System.IO;
using System;
using System.Reflection;

namespace NGraphics.Test
{
	[TestFixture]
	public class SvgReaderTests : PlatformTest
	{
		Graphic Read (string path)
		{
			using (var s = OpenResource (path)) {
				var r = new SvgReader (new StreamReader (s));
				Assert.GreaterOrEqual (r.Graphic.Children.Count, 0);
				Assert.Greater (r.Graphic.Size.Width, 1);
				Assert.Greater (r.Graphic.Size.Height, 1);
				return r.Graphic;
			}
		}

		void ReadAndDraw (string path)
		{
			var g = Read (path);
			var c = Platform.CreateImageCanvas (g.Size);
			g.Draw (c);
			c.GetImage ().SaveAsPng (GetPath (path));
		}

		[Test]
		public void MozillaEllipse ()
		{
			ReadAndDraw ("mozilla.ellipse.svg");
		}

		[Test]
		public void MozillaPath ()
		{
			ReadAndDraw ("mozilla.path.svg");
		}

		[Test]
		public void MozillaTransform ()
		{
			ReadAndDraw ("mozilla.transform.svg");
		}

		[Test]
		public void MozillaBezierCurves1 ()
		{
			ReadAndDraw ("mozilla.BezierCurves1.svg");
		}

		[Test]
		public void MozillaBezierCurves2 ()
		{
			ReadAndDraw ("mozilla.BezierCurves2.svg");
		}

		[Test]
		public void MozillaText1 ()
		{
			ReadAndDraw ("mozilla.Text1.svg");
		}

		[Test]
		public void MozillaText2 ()
		{
			ReadAndDraw ("mozilla.Text2.svg");
		}

		[Test]
		public void MozillaText3 ()
		{
			ReadAndDraw ("mozilla.Text3.svg");
		}

		[Test]
		public void MozillaText4 ()
		{
			ReadAndDraw ("mozilla.Text4.svg");
		}

		[Test]
		public void Smile ()
		{
			ReadAndDraw ("Smile.svg");
		}

		[Test]
		public void SunAtNight ()
		{
			ReadAndDraw ("SunAtNight.svg");
		}

		[Test]
		public void MocastIcon ()
		{
			ReadAndDraw ("MocastIcon.svg");
		}

		[Test]
		public void ErulisseuiinSpaceshipPack ()
		{
			ReadAndDraw ("ErulisseuiinSpaceshipPack.svg");
		}
	}
}

