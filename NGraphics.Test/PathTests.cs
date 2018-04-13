#if VSTEST
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
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
	}
}

