using NUnit.Framework;
using System;

namespace NGraphics.Test
{
	[TestFixture]
	public class PrimitivesTest
	{
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
			Assert.NotNull (g);
		}
	}
}

