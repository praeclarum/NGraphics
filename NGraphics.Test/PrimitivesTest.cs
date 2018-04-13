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
	public class PrimitivesTest
	{
		[Test]
		public void RectInflate ()
		{
			var r = new Rect (new Size (10, 20));
			r.Inflate (4, 6);
			Assert.AreEqual (-4, r.X);
			Assert.AreEqual (-6, r.Y);
			Assert.AreEqual (18, r.Width);
			Assert.AreEqual (32, r.Height);
		}

		[Test]
		public void PointMinusScalar ()
		{
			var r = new Point (1, 2) - 3;
			Assert.AreEqual (new Point (-2, -1), r);
		}

		[Test]
		public void PointPlusScalar ()
		{
			var r = new Point (1, 2) + 3;
			Assert.AreEqual (new Point (4, 5), r);
		}

		[Test]
		public void CreateOval ()
		{
			var g = new Ellipse (new Point (10, 20), new Size (30, 40));
			Assert.IsNotNull (g);
		}
	}
}

