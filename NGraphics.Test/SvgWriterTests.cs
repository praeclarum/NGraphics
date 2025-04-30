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
	public class SvgWriterTests : PlatformTest
	{
		[Test]
		public void GetUtf8 ()
		{
			var svg = new GraphicCanvas ().Graphic.GetSvg ();
			ClassicAssert.IsTrue (svg.Contains ("encoding=\"UTF-8\""));
		}

		[Test]
		public void RoundedRect ()
		{
			var c = new GraphicCanvas ();
			c.DrawRectangle (new Rect (0, 0, 40, 20), new Size (3, 3), null, new SolidBrush ("#333"));
			var svg = c.Graphic.GetSvg ();
			ClassicAssert.IsTrue (svg.Contains ("rx=\"3\""));
			ClassicAssert.IsTrue (svg.Contains ("ry=\"3\""));
		}

		[Test]
		public void NormalRect ()
		{
			var c = new GraphicCanvas ();
			c.DrawRectangle (new Rect (0, 0, 40, 20), new Size (0, 0), null, new SolidBrush ("#333"));
			var svg = c.Graphic.GetSvg ();
			ClassicAssert.IsTrue (!svg.Contains ("rx="));
			ClassicAssert.IsTrue (!svg.Contains ("ry="));
		}

		//[Test]
		public void AutoSize ()
		{
			var c = new GraphicCanvas ();
			c.DrawRectangle (new Rect (0, 0, 40, 20), new Size (3, 3), null, new SolidBrush ("#333"));
			var svg = c.Graphic.GetSvg ();
			ClassicAssert.IsTrue (svg.Contains("svg width=\"40\""));

		}

		[Test]
		public void NoTitle ()
		{
			var c = new GraphicCanvas ();
			c.DrawRectangle (new Rect (0, 0, 40, 20), new Size (3, 3), null, new SolidBrush ("#333"));
			var svg = c.Graphic.GetSvg ();
			ClassicAssert.IsTrue (!svg.Contains ("<title>"));
			ClassicAssert.IsTrue (!svg.Contains ("<description>"));
		}
	}
}

