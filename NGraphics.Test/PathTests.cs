using NUnit.Framework;
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

			Assert.False (p.Contains (new Point (0, 1)));
			Assert.True (p.Contains (new Point (0, 0)));
			Assert.True (p.Contains (new Point (99, 49)));
			Assert.False (p.Contains (new Point (50, 49)));
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

