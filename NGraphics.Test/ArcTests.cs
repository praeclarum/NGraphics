#if VSTEST
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#else
using NUnit.Framework;
#endif
using System;
using System.Threading.Tasks;

namespace NGraphics.Test
{
	[TestFixture]
	public class ArcTests: PlatformTest
	{
		// See example of Large+Sweep differences
		// https://docs.oracle.com/javase/8/javafx/api/javafx/scene/shape/ArcTo.html
		private Task DrawTutorial (string id, bool large, bool sweep)
		{
			var canvas = Platform.CreateImageCanvas (new Size (200), transparency: true);
			var path = new PathOp[]
			{
				new MoveTo (100, 100),
				new ArcTo (new Size (40, 40), large, sweep, new Point (125, 125)),
				new ClosePath(),
			};
			canvas.DrawPath (path, null, Brushes.Black);
			return SaveImage (canvas, $"Arc.Tutorial.{id}");
		}

		[Test]
		public Task ArcTutorial1 ()
		{
			return DrawTutorial ("1", false, false);
		}

		[Test]
		public Task ArcTutorial2 ()
		{
			return DrawTutorial ("2", false, true);
		}

		[Test]
		public Task ArcTutorial3 ()
		{
			return DrawTutorial ("3", true, false);
		}

		[Test]
		public Task ArcTutorial4 ()
		{
			return DrawTutorial ("4", true, true);
		}

		[Test]
		public async Task HalfCircleClockwise ()
		{
			var canvas = Platform.CreateImageCanvas (new Size (100), transparency: true);
			var path = new PathOp[]
			{
				new MoveTo (0, 50),
				new LineTo (100, 50),
				new ArcTo (new Size (50), true, true, new Point (0, 50)),
				new ClosePath(),
			};
			canvas.DrawPath (path, null, Brushes.Black);
			await SaveImage (canvas, "Arc.HalfCircle.Clockwise");
		}

		[Test]
		public async Task HalfCircleCounterClockwise ()
		{
			var canvas = Platform.CreateImageCanvas (new Size (100), transparency: true);
			var path = new PathOp[]
			{
				new MoveTo (0, 50),
				new LineTo (100, 50),
				new ArcTo (new Size (50), false, false, new Point (0, 50)),
				new ClosePath(),
			};
			canvas.DrawPath (path, null, Brushes.Black);
			await SaveImage (canvas, "Arc.HalfCircle.CounterClockwise");
		}

		[Test]
		public async Task SmallArc ()
		{
			var canvas = Platform.CreateImageCanvas (new Size (100), transparency: true);
			var path = new PathOp[]
			{
				new MoveTo (0, 50),
				new LineTo (100, 50),
				new ArcTo (new Size (50), false, false, new Point (50, 100)),
				new LineTo (0, 50),
				new ClosePath(),
			};
			canvas.DrawPath (path, null, Brushes.Black);
			await SaveImage (canvas, "Arc.Small");
		}

		[Test]
		public async Task SmallSweepClockwiseArc ()
		{
			var canvas = Platform.CreateImageCanvas (new Size (100), transparency: true);
			var path = new PathOp[]
			{
				new MoveTo (0, 50),
				new LineTo (100, 50),
				new ArcTo (new Size (50), false, true, new Point (50, 100)),
				new LineTo (0, 50),
				new ClosePath(),
			};
			canvas.DrawPath (path, null, Brushes.Black);
			await SaveImage (canvas, "Arc.Small.Sweep");
		}

		[Test]
		public async Task LargeArc ()
		{
			var canvas = Platform.CreateImageCanvas (new Size (100), transparency: true);
			var path = new PathOp[]
			{
				new MoveTo (0, 50),
				new LineTo (100, 50),
				new ArcTo (new Size (50), true, false, new Point (50, 100)),
				new LineTo (0, 50),
				new ClosePath(),
			};
			canvas.DrawPath (path, null, Brushes.Black);
			await SaveImage (canvas, "Arc.Large");
		}

		[Test]
		public async Task LargeSweepClockwiseArc ()
		{
			var canvas = Platform.CreateImageCanvas (new Size (200), transparency: true);
			var path = new PathOp[]
			{
				new MoveTo (0, 50),
				new LineTo (100, 50),
				new ArcTo (new Size (50), true, true, new Point (50, 100)),
				new LineTo (0, 50),
				new ClosePath(),
			};
			canvas.DrawPath (path, null, Brushes.Black);
			await SaveImage (canvas, "Arc.Large.Sweep");
		}
	}
}
