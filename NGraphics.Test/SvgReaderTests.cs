#if VSTEST
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
		Graphic Read (string path, Brush defaultBrush = null)
		{
			using (var s = OpenResource (path)) {
				var r = new SvgReader (new StreamReader (s), defaultBrush: defaultBrush);
				Assert.IsTrue (r.Graphic.Children.Count >= 0);
				Assert.IsTrue (r.Graphic.Size.Width > 1);
				Assert.IsTrue (r.Graphic.Size.Height > 1);
				return r.Graphic;
			}
		}

		Graphic ReadString (string svg, Brush defaultBrush = null)
		{
			var r = new SvgReader (svg, defaultBrush: defaultBrush);
			Assert.IsTrue (r.Graphic.Children.Count >= 0);
			return r.Graphic;
		}

		async Task ReadAndDraw (string path, Brush defaultBrush = null)
		{
			var g = Read (path, defaultBrush);

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
		public void RelativeMoveAfterClose ()
		{
			var g = ReadString ("<svg><path d=\"M1,2L3,4zm100,100\"/></svg>");
			var p = (Path)g.Children[0];
			Assert.AreEqual (4, p.Operations.Count);
			var m = p.Operations[3];
			Assert.AreEqual (new Point (103, 104), m.EndPoint);
		}

		[Test]
		public async Task Find ()
		{
			// Issue #91
			await ReadAndDraw ("find.svg");
		}

		[Test]
		public async Task EvenOdd ()
		{
			await ReadAndDraw ("EvenOdd.svg");
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

		[Test]
		public void ReadPathWithoutWhite_Issue77 ()
		{
			var svg = @"
			<svg xmlns=""http://www.w3.org/2000/svg"" width=""48"" height=""48"" viewBox=""0 0 48 48"">
	<path d=""M13.25 21.59c2.88 5.66 7.51 10.29 13.18 13.17l4.4-4.41c.55-.55 1.34-.71 2.03-.49C35.1 30.6 37.51 31 40 31c1.11 0 2 .89 2 2v7c0 1.11-.89 2-2 2C21.22 42 6 26.78 6 8c0-1.11.9-2 2-2h7c1.11 0 2 .89 2 2 0 2.49.4 4.9 1.14 7.14.22.69.06 1.48-.49 2.03l-4.4 4.42z"" fill=""#000000"" fill-opacity=""0.54"" />
</svg>";
			var g = ReadString (svg);
			Assert.AreEqual (1, g.Children.Count);
			Assert.IsTrue (g.Children[0] is Path);
			Assert.AreEqual (16, ((Path)g.Children[0]).Operations.Count);
		}
	}
}

