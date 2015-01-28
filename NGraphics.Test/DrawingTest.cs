using NUnit.Framework;
using System;

namespace NGraphics.Test
{
	[TestFixture]
	public class DrawingTest
	{
		[Test]
		public void Oval ()
		{
			var d = new Drawing (s => {

				s.DrawOval (new Point (10, 20), new Size (30, 40), Pens.Black);

			});
			Assert.AreEqual (1, d.NumChildren);
		}

		[Test]
		public void TwoOvals ()
		{
			var d = new Drawing (s => {
				s.DrawOval (new Point (10, 20), new Size (30, 40), Pens.Black);
				s.DrawOval (new Point (20, 30), new Size (40, 30), Pens.Black);
			});
			Assert.AreEqual (2, d.NumChildren);
		}

		[Test]
		public void RefreshWithFunc ()
		{
			var num = 1;
			var d = new Drawing (s => {
				for (var i = 0; i < num; i++) {
					s.DrawOval (new Point (10*i, 20), new Size (30, 40), Pens.Black);
				}
			});
			Assert.AreEqual (1, d.NumChildren);

			num = 2;
			d.Refresh ();
			Assert.AreEqual (2, d.NumChildren);
		}

		[Test]
		public void EmptyDrawing ()
		{
			var d = new Drawing ();
			Assert.AreEqual (0, d.NumChildren);
		}

		[Test]
		public void Refresh ()
		{
			var d = new Drawing ();
			d.DrawOval (new Point (10, 20), new Size (30, 40), Pens.Black);
			Assert.AreEqual (1, d.NumChildren);

			d.Refresh ();
			Assert.AreEqual (0, d.NumChildren);
		}

		[Test]
		public void Circular ()
		{
			var d = new Drawing (s => {
				s.DrawOval (new Point (10, 20), new Size (30, 40), Pens.Black);
				s.DrawOval (new Point (20, 30), new Size (40, 30), Pens.Black);
			});

			var d2 = new Drawing (d.Draw);

			Assert.AreEqual (2, d.NumChildren);
			Assert.AreEqual (2, d2.NumChildren);
		}
	}
}

