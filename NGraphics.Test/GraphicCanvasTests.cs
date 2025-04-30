﻿#if VSTEST
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.AppContainer.UITestMethodAttribute;
#else
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
			ClassicAssert.AreEqual (1f, d.Graphic.Children.Count);
		}
	}
}

