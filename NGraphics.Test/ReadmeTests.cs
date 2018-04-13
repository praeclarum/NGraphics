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
using System.Threading.Tasks;

namespace NGraphics.Test
{
	[TestFixture]
	public class ReadmeTests : PlatformTest
	{
		// http://app.coolors.co/dcdcdd-c5c3c6-46494c-4c5c68-4183c4
		// http://app.coolors.co/dcdcdd-c5c3c6-46494c-4c5c68-68a5e2
		[Test]
		public async Task Icon ()
		{
			var size = new Size (64);
			var canvas = Platforms.Current.CreateImageCanvas (size, scale: 2);
			canvas.SaveState ();
			canvas.Scale (size);
			canvas.Translate (1 / 8.0, 0);

			var p = new Path ();
			p.MoveTo (0, 1);
			p.LineTo (0, 0);
			p.LineTo (0.5, 1);
			p.LineTo (0.5, 0);

			var colors = new [] {
				"#DCDCDD",
				"#C5C3C6",
				"#46494C",
				"#4C5C68",
				"#68A5E2",
			};
			foreach (var c in colors) {
				p.Pen = new Pen (c, 1 / 4.0);
				p.Draw (canvas);
				canvas.Translate (1 / 16.0, 0);
			}

			await SaveImage (canvas, "Icon.png");
		}

		[Test]
		public async Task Example1 ()
		{
			var canvas = Platforms.Current.CreateImageCanvas (new Size (100), scale: 2);

			var skyBrush = new LinearGradientBrush (Point.Zero, Point.OneY, Colors.Blue, Colors.White);
			canvas.FillRectangle (new Rect (canvas.Size), skyBrush);
			canvas.FillEllipse (10, 10, 30, 30, Colors.Yellow);
			canvas.FillRectangle (50, 60, 60, 40, Colors.LightGray);
			canvas.FillPath (new PathOp[] {	
				new MoveTo (40, 60),
				new LineTo (120, 60),
				new LineTo (80, 30),
				new ClosePath ()
			}, Colors.Gray);

			await SaveImage (canvas, "Example1.png");
		}

		[Test]
		public async Task PenWidths ()
		{
			var canvas = Platforms.Current.CreateImageCanvas (new Size (120*5, 120), scale: 2);

			canvas.Translate (20, 20);
			for (var i = 0; i < 5; i++) {
				canvas.DrawEllipse (
					new Rect (new Size (80)),
					pen: Pens.DarkGray.WithWidth (1 << i),
					brush: Brushes.LightGray);
				canvas.Translate (120, 0);
			}

			await SaveImage (canvas, "PenWidths.png");
		}
	}

}

