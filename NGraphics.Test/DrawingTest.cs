#if VSTEST
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.AppContainer.UITestMethodAttribute;
#else
using NUnit.Framework;
#endif
using System;

namespace NGraphics.Test
{
	[TestFixture]
	public class DrawingTest
	{
		[Test]
		public void Ellipse ()
		{
			var d = new Drawing (new Size (50, 50), s => {

				s.DrawEllipse (new Point (10, 20), new Size (30, 40), Pens.Black);

			});
			Assert.AreEqual (1, d.Graphic.Children.Count);
		}

		[Test]
		public void TwoEllipses ()
		{
			var d = new Drawing (new Size (50, 50), s => {
				s.DrawEllipse (new Point (10, 20), new Size (30, 40), Pens.Black);
				s.DrawEllipse (new Point (20, 30), new Size (40, 30), Pens.Black);
			});
			Assert.AreEqual (2, d.Graphic.Children.Count);
		}

		[Test]
		public void RefreshWithFunc ()
		{
			var num = 1;
			var d = new Drawing (new Size (50, 50), s => {
				for (var i = 0; i < num; i++) {
					s.DrawEllipse (new Point (10*i, 20), new Size (30, 40), Pens.Black);
				}
			});
			Assert.AreEqual (1, d.Graphic.Children.Count);

			num = 2;
			d.Invalidate ();
			Assert.AreEqual (2, d.Graphic.Children.Count);
		}

		[Test]
		public void EmptyDrawing ()
		{
			var d = new Drawing (new Size (50, 50), s => {
			});
			Assert.AreEqual (0, d.Graphic.Children.Count);
		}
	}
}

