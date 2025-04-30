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

namespace NGraphics.Test
{
	[TestFixture]
	public class GroupTests : PlatformTest
	{
		[Test]
		public void EmptyBoundingBox ()
		{
			var g = new Group ();
			var bb = g.BoundingBox;
			ClassicAssert.IsFalse (bb.HasValue);
		}

		[Test]
		public void RectBoundingBox ()
		{
			var g = new Group ();
			var c0 = new Rectangle (new Rect (-10, -20, 100, 200));
			g.Children.Add (c0);
			var bb = g.BoundingBox;
			ClassicAssert.AreEqual (-10, bb.Value.X);
			ClassicAssert.AreEqual (90, bb.Value.Right);
			ClassicAssert.AreEqual (-20, bb.Value.Y);
			ClassicAssert.AreEqual (180, bb.Value.Bottom);
		}

		[Test]
		public void TanslatedRectBoundingBox ()
		{
			var g = new Group ();
			var c0 = new Rectangle (new Rect (-10, -20, 100, 200));
			c0.Transform = Transform.Translate (500, 0);
			g.Children.Add (c0);
			var bb = g.BoundingBox;
			ClassicAssert.AreEqual (500-10, bb.Value.X);
			ClassicAssert.AreEqual (500+90, bb.Value.Right);
			ClassicAssert.AreEqual (-20, bb.Value.Y);
			ClassicAssert.AreEqual (180, bb.Value.Bottom);
		}
	}
}

