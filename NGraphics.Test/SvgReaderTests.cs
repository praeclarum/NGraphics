﻿#if VSTEST
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.AppContainer.UITestMethodAttribute;
#else
using NUnit.Framework;
#endif
using System.IO;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace NGraphics.Test
{
	[TestFixture]
	public class SvgReaderTests : PlatformTest
	{
		Graphic Read (string path)
		{
			using (var s = OpenResource (path)) {
				var r = new SvgReader (new StreamReader (s));
				Assert.IsTrue (r.Graphic.Children.Count >= 0);
				Assert.IsTrue (r.Graphic.Size.Width > 1);
				Assert.IsTrue (r.Graphic.Size.Height > 1);
				return r.Graphic;
			}
		}

		async Task ReadAndDraw (string path)
		{
			var g = Read (path);

			//
			// Draw Image
			//
			var c = Platform.CreateImageCanvas (g.Size);
			g.Draw (c);
			await SaveImage (c, path);

			var c2 = new GraphicCanvas (g.Size, Platform);
			g.Draw (c2);
			await SaveSvg (c2, path);
		}

		[Test]
		public async Task MozillaEllipse ()
		{
			await ReadAndDraw ("mozilla.ellipse.svg");
		}

		[Test]
		public async Task MozillaPath ()
		{
			await ReadAndDraw ("mozilla.path.svg");
		}

		[Test]
		public async Task MozillaTransform ()
		{
			await ReadAndDraw ("mozilla.transform.svg");
		}

		[Test]
		public async Task MozillaBezierCurves1 ()
		{
			await ReadAndDraw ("mozilla.BezierCurves1.svg");
		}

		[Test]
		public async Task MozillaBezierCurves2 ()
		{
			await ReadAndDraw ("mozilla.BezierCurves2.svg");
		}

		[Test]
		public async Task MozillaText1 ()
		{
			await ReadAndDraw ("mozilla.Text1.svg");
		}

		[Test]
		public async Task MozillaText2 ()
		{
			await ReadAndDraw ("mozilla.Text2.svg");
		}

		[Test]
		public async Task MozillaText3 ()
		{
			await ReadAndDraw ("mozilla.Text3.svg");
		}

		[Test]
		public async Task MozillaText4 ()
		{
			await ReadAndDraw ("mozilla.Text4.svg");
		}

		[Test]
		public async Task Smile ()
		{
			await ReadAndDraw ("Smile.svg");
		}

		[Test]
		public async Task SunAtNight ()
		{
			await ReadAndDraw ("SunAtNight.svg");
		}

		[Test]
		public async Task MocastIcon ()
		{
			await ReadAndDraw ("MocastIcon.svg");
		}

		[Test]
		public async Task ErulisseuiinSpaceshipPack ()
		{
			await ReadAndDraw ("ErulisseuiinSpaceshipPack.svg");
		}

		[Test]
		public async Task Repeat ()
		{
			await ReadAndDraw ("repeat.svg");
		}

		[Test]
		public async Task SliderThumb ()
		{
			await ReadAndDraw ("sliderThumb.svg");
		}

		[Test]
		public async Task TextVariations ()
		{
			await ReadAndDraw ("TextVariations.svg");
		}

		[Test]
		public async Task GroupOpacity ()
		{
			await ReadAndDraw ("GroupOpacity.svg");
		}
	}
}

