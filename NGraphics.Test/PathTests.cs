#if VSTEST
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.AppContainer.UITestMethodAttribute;
#else
using NUnit.Framework;
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

			Assert.IsFalse (p.Contains (new Point (0, 1)));
			Assert.IsTrue (p.Contains (new Point (0, 0)));
			Assert.IsTrue (p.Contains (new Point (99, 49)));
			Assert.IsFalse (p.Contains (new Point (50, 49)));
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
			Assert.AreEqual (100, bb.Value.X);
			Assert.AreEqual (200, bb.Value.Right);
			Assert.AreEqual (200, bb.Value.Y);
			Assert.AreEqual (300, bb.Value.Bottom);
		}

		[Test]
		public void EmptyBoundingBox ()
		{
			var p = new Path ();
			p.Close ();
			var bb = p.BoundingBox;
			Assert.IsFalse (bb.HasValue);
		}

		[Test]
		public void OnlyMoveBoundingBox ()
		{
			var p = new Path ();
			p.MoveTo (new Point (100, 200));
			var bb = p.BoundingBox;
			Assert.AreEqual (100, bb.Value.X);
			Assert.AreEqual (100, bb.Value.Right);
			Assert.AreEqual (200, bb.Value.Y);
			Assert.AreEqual (200, bb.Value.Bottom);
		}
	}
}

