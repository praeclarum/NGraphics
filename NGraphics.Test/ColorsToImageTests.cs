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
	public class ColorsToImageTests : PlatformTest
	{
		[Test]
		public async Task RGBY ()
		{
			var image = Platform.CreateImage (
				new[] { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow },
				2);
			await SaveImage (image, "ColorToImage.RGBY.png");
		}

		[Test]
		public async Task Rand ()
		{
			var rand = new Random (1);

			var image = Platforms.Current.CreateImage ((x, y) => {

				return new Color (rand.NextDouble (), rand.NextDouble (), rand.NextDouble (), 1);

			}, new Size (64), scale: 1);

			await SaveImage (image, "ColorToImage.Rand.png");
		}

		[Test]
		public async Task RandAlpha ()
		{
			var rand = new Random (1);

			var image = Platforms.Current.CreateImage ((x, y) => {

				return new Color (rand.NextDouble (), rand.NextDouble (), rand.NextDouble (), rand.NextDouble ());

			}, new Size (64), scale: 1);

			await SaveImage (image, "ColorToImage.RandAlpha.png");
		}

		[Test]
		public async Task Mandelbrot ()
		{
			var w = 600.0;
			var h = w * (2 / 3.5);
			var image = Platforms.Current.CreateImage ((px, py) => {

//				var x0 = scaled x coordinate of pixel (scaled to lie in the Mandelbrot X scale (-2.5, 1));
//				var y0 = scaled y coordinate of pixel (scaled to lie in the Mandelbrot Y scale (-1, 1));
				var x0 = px / w * (3.5) - 2.5;
				var y0 = py / h * (2) - 1;
				var x = 0.0;
				var y = 0.0;
				var iteration = 0;
				const int maxIterations = 1000;
				while (x*x + y*y < 2*2 && iteration < maxIterations) {
					var xtemp = x*x - y*y + x0;
					y = 2*x*y + y0;
					x = xtemp;
					iteration = iteration + 1;
				}
				return new Color (iteration / (double)maxIterations, 1);

			}, new Size (w, h), scale: 1);

			await SaveImage (image, "ColorToImage.Mandelbrot.png");
		}

		[Test]
		public async Task Circle ()
		{
			var d = 64;
			var r2 = (d / 2) * (d / 2);
			var image = Platforms.Current.CreateImage ((px, py) => {

				var dx = px - d / 2;
				var dy = py - d  /2;
				var p2 = dx*dx + dy*dy;
				if (p2 <= r2)
					return Colors.Yellow;
				return Colors.Black;

			}, new Size (d + 1, d + 2), scale: 1);

			await SaveImage (image, "ColorToImage.Circle.png");
		}

		[Test]
		public async Task Grid ()
		{
			var d = 200;
			var image = Platforms.Current.CreateImage ((px, py) => {

				if ((px % 10) == 0)
					return Colors.Blue;
				if ((py % 10) == 0)
					return Colors.Yellow;
				return Colors.DarkGray;

			}, new Size (d, d), scale: 1);

			await SaveImage (image, "ColorToImage.Grid.png");
		}
	}
}

