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
	public class PathTests : PlatformTest
	{
		[Test]
		public void Contains ()
		{
			var p = new Path ();
			p.MoveTo (new Point (0, 0));
			p.LineTo (new Point (100, 0));
			p.LineTo (new Point (100, 50));
			p.Close ();

			ClassicAssert.IsFalse (p.Contains (new Point (0, 1)));
			ClassicAssert.IsTrue (p.Contains (new Point (0, 0)));
			ClassicAssert.IsTrue (p.Contains (new Point (99, 49)));
			ClassicAssert.IsFalse (p.Contains (new Point (50, 49)));
		}

		[Test]
		public void TurtleGraphics ()
		{
			var p = new Path ();
			p.MoveTo (new Point (100, 200));
			p.LineTo (new Point (200, 250));
			p.LineTo (new Point (100, 300));
			p.Close ();
		}

		[Test]
		public void LinesBoundingBox ()
		{
			var p = new Path ();
			p.MoveTo (new Point (100, 200));
			p.LineTo (new Point (200, 250));
			p.LineTo (new Point (100, 300));
			p.Close ();
			var bb = p.BoundingBox;
			ClassicAssert.AreEqual (100, bb.Value.X);
			ClassicAssert.AreEqual (200, bb.Value.Right);
			ClassicAssert.AreEqual (200, bb.Value.Y);
			ClassicAssert.AreEqual (300, bb.Value.Bottom);
		}

		[Test]
		public void EmptyBoundingBox ()
		{
			var p = new Path ();
			p.Close ();
			var bb = p.BoundingBox;
			ClassicAssert.IsFalse (bb.HasValue);
		}

		[Test]
		public void OnlyMoveBoundingBox ()
		{
			var p = new Path ();
			p.MoveTo (new Point (100, 200));
			var bb = p.BoundingBox;
			ClassicAssert.AreEqual (100, bb.Value.X);
			ClassicAssert.AreEqual (100, bb.Value.Right);
			ClassicAssert.AreEqual (200, bb.Value.Y);
			ClassicAssert.AreEqual (200, bb.Value.Bottom);
		}
	}
}

