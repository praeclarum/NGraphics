#if VSTEST
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#else
using NUnit.Framework;
#endif
using System;

namespace NGraphics.Test
{
	[TestFixture]
	public class GraphicCanvasTests
	{
		[Test]
		public void Ellipse ()
		{
			var d = new GraphicCanvas (new Size (50, 50));
			d.DrawEllipse (new Point (10, 20), new Size (30, 40), Pens.Black);
			Assert.AreEqual (1, d.Graphic.Children.Count);
		}
	}
}

