#if VSTEST
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#else
using NUnit.Framework;
#endif
using System;

namespace NGraphics.Test
{
	[TestFixture]
	public class GraphicCanvasTests : PlatformTest
	{
		[Test]
		public void Ellipse ()
		{
			var d = new GraphicCanvas (new Size (50, 50), Platform);
			d.DrawEllipse (new Point (10, 20), new Size (30, 40), Pens.Black);
			Assert.AreEqual (1f, d.Graphic.Children.Count);
		}
	}
}

