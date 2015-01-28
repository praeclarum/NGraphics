using NUnit.Framework;
using System;

namespace XGraphics.Test
{
	[TestFixture]
	public class DrawingTest
	{
		[Test]
		public void StrokeOval ()
		{
			var d = new Drawing (s => {

				s.DrawOval (new Point (10, 20), new Size (30, 40), Pen.Black);

			});
			Assert.AreEqual (1, d.NumChildren);
		}

		[Test]
		public void StrokeTwoOvals ()
		{
			var d = new Drawing (s => {

				s.DrawOval (new Point (10, 20), new Size (30, 40), Pen.Black);
				s.DrawOval (new Point (20, 30), new Size (40, 30), Pen.Black);

			});
			Assert.AreEqual (2, d.NumChildren);
		}
	}
}

