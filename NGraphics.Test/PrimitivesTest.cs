#if VSTEST
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
	public class PrimitivesTest
	{
		[Test]
		public void RectInflate ()
		{
			var r = new Rect (new Size (10, 20));
			r.Inflate (4, 6);
			ClassicAssert.AreEqual (-4, r.X);
			ClassicAssert.AreEqual (-6, r.Y);
			ClassicAssert.AreEqual (18, r.Width);
			ClassicAssert.AreEqual (32, r.Height);
		}

		[Test]
		public void PointMinusScalar ()
		{
			var r = new Point (1, 2) - 3;
			ClassicAssert.AreEqual (new Point (-2, -1), r);
		}

		[Test]
		public void PointPlusScalar ()
		{
			var r = new Point (1, 2) + 3;
			ClassicAssert.AreEqual (new Point (4, 5), r);
		}

		[Test]
		public void CreateOval ()
		{
			var g = new Ellipse (new Point (10, 20), new Size (30, 40));
			ClassicAssert.IsNotNull (g);
		}
	}
}

