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
	public class DrawingTest : PlatformTest
	{
		[Test]
		public void Ellipse ()
		{
			var d = new Drawing (new Size (50, 50), s => {

				s.DrawEllipse (new Point (10, 20), new Size (30, 40), Pens.Black);

			}, Platform);
			Assert.AreEqual (1f, d.Graphic.Children.Count);
		}

		[Test]
		public void TwoEllipses ()
		{
			var d = new Drawing (new Size (50, 50), s => {
				s.DrawEllipse (new Point (10, 20), new Size (30, 40), Pens.Black);
				s.DrawEllipse (new Point (20, 30), new Size (40, 30), Pens.Black);
			}, Platform);
			Assert.AreEqual (2f, d.Graphic.Children.Count);
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
			Assert.AreEqual (1f, d.Graphic.Children.Count);

			num = 2;
			d.Invalidate ();
			Assert.AreEqual (2f, d.Graphic.Children.Count);
		}

		[Test]
		public void EmptyDrawing ()
		{
			var d = new Drawing (new Size (50, 50), s => {
			}, Platform);
			Assert.AreEqual (0f, d.Graphic.Children.Count);
		}
	}
}

