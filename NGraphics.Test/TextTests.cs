#if VSTEST
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.AppContainer.UITestMethodAttribute;
#else
using NUnit.Framework;
using NUnit.Framework.Legacy;
#endif
using System.IO;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace NGraphics.Test
{
	[TestFixture]
	public class TextTests : PlatformTest
	{
		async Task Draw (GraphicCanvas canvas, string path)
		{
			var g = canvas.Graphic;

			//
			// Draw Image
			//
			var c = Platform.CreateImageCanvas (g.Size);
			g.Draw (c);
			await SaveImage (c, path);

			var c2 = Platform.CreateGraphicCanvas (g.Size);
			g.Draw (c2);
			await SaveSvg (c2, path);
		}

		async Task Draw (Action<ICanvas> draw, Size size, string path)
		{
			var c = Platform.CreateImageCanvas (size);
			draw (c);
			await SaveImage (c, path);

			var c2 = Platform.CreateGraphicCanvas (size);
			draw (c2);
			await SaveSvg (c2, path);
		}

		void DrawTextBox (ICanvas c, string text, Point point, Font f)
		{
			var b = new SolidBrush (Colors.Black);
			var bp = new Pen (Colors.Blue);
			var rp = new Pen (Colors.Red);
			var size = c.MeasureText (text, f);
			c.DrawRectangle (new Rect (point, size.Size) + new Size (0, -size.Ascent), bp);
			c.DrawLine (point, point + new Size (size.Width, 0), rp);
			c.DrawText (text, point, f, b);
		}

		[Test]
		public async Task DrawHelloWorld ()
		{
			await Draw (c => {

				var f = new Font {
					Size = 20,
				};
				DrawTextBox (c, "Hello World", new Point (0, 20), f);

			}, new Size (256), "TextTests.HelloWorld");
		}

		[Test]
		public async Task DrawHelloWorldLineBreak ()
		{
			await Draw (c => {

				var f = new Font {
					Size = 20,
				};
				DrawTextBox (c, "Hello\nWorld", new Point (0, 20), f);

			}, new Size (256), "TextTests.HelloWorldLineBreak");
		}

		[Test]
		public async Task DrawDescents ()
		{
			await Draw (c => {

				var f = new Font {
					Size = 20,
				};
				DrawTextBox (c, "qypfgj EM", new Point (0, 40), f);

			}, new Size (256), "TextTests.Descents");
		}

		[Test]
		public void NullMeasure20 ()
		{
			var text = "Hello World!";
			var font = new Font ("Helvetica", 20);

			var nm = NullPlatform.GlobalMeasureText (text, font);
			var pm = Platform.MeasureText (text, font);

			ClassicAssert.AreEqual (pm.Ascent, nm.Ascent, 1.0e-2);
			ClassicAssert.AreEqual (pm.Descent, nm.Descent, 1.0e-2);
			ClassicAssert.AreEqual (pm.Width, nm.Width, pm.Width * 0.01);
		}

		[Test]
		public void NullMeasure20Newlines ()
		{
			var text = "Hello World!\nBye\nbye\n";
			var font = new Font ("Helvetica", 20);

			var nm = NullPlatform.GlobalMeasureText (text, font);
			var pm = Platform.MeasureText (text, font);

			ClassicAssert.AreEqual (pm.Ascent, nm.Ascent, 1.0e-2);
			ClassicAssert.AreEqual (pm.Descent, nm.Descent, 1.0e-2);
			ClassicAssert.AreEqual (pm.Width, nm.Width, pm.Width * 0.2);
		}

		[Test]
		public void NullMeasure8 ()
		{
			var text = "Hello World!";
			var font = new Font ("Helvetica", 8);

			var nm = NullPlatform.GlobalMeasureText (text, font);
			var pm = Platform.MeasureText (text, font);

			ClassicAssert.AreEqual (pm.Ascent, nm.Ascent, 0.01);
			ClassicAssert.AreEqual (pm.Descent, nm.Descent, 0.01);
			ClassicAssert.AreEqual (pm.Width, nm.Width, pm.Width * 0.01);
		}
	}
}

