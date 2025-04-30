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
	public class DrawingTest : PlatformTest
	{
		[Test]
		public void Ellipse ()
		{
			var d = new Drawing (new Size (50, 50), s => {

				s.DrawEllipse (new Point (10, 20), new Size (30, 40), Pens.Black);

			}, Platform);
			ClassicAssert.AreEqual (1f, d.Graphic.Children.Count);
		}

		[Test]
		public void TwoEllipses ()
		{
			var d = new Drawing (new Size (50, 50), s => {
				s.DrawEllipse (new Point (10, 20), new Size (30, 40), Pens.Black);
				s.DrawEllipse (new Point (20, 30), new Size (40, 30), Pens.Black);
			}, Platform);
			ClassicAssert.AreEqual (2f, d.Graphic.Children.Count);
		}

		[Test]
		public void RefreshWithFunc ()
		{
			var num = 1;
			var d = new Drawing (new Size (50, 50), s => {
				for (var i = 0; i < num; i++) {
					s.DrawEllipse (new Point (10*i, 20), new Size (30, 40), Pens.Black);
				}
			}, Platform);
			ClassicAssert.AreEqual (1f, d.Graphic.Children.Count);

			num = 2;
			d.Invalidate ();
			ClassicAssert.AreEqual (2f, d.Graphic.Children.Count);
		}

		[Test]
		public void EmptyDrawing ()
		{
			var d = new Drawing (new Size (50, 50), s => {
			}, Platform);
			ClassicAssert.AreEqual (0f, d.Graphic.Children.Count);
		}
	}
}

