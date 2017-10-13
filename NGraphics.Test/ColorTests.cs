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
	public class ColorTests : PlatformTest
	{
		[Test]
		public async Task HSL ()
		{
			// http://en.wikipedia.org/wiki/HSL_and_HSV#Examples
			var tests = new [] {
				Tuple.Create (new Color (1.000, 1.000, 1.000), Color.FromHSL (  0.0/360.0, 0.000, 1.000)),
				Tuple.Create (new Color (0.500, 0.500, 0.500), Color.FromHSL (  0.0/360.0, 0.000, 0.500)),
				Tuple.Create (new Color (0.000, 0.000, 0.000), Color.FromHSL (  0.0/360.0, 0.000, 0.000)),
				Tuple.Create (new Color (1.000, 0.000, 0.000), Color.FromHSL (  0.0/360.0, 1.000, 0.500)),
				Tuple.Create (new Color (0.750, 0.750, 0.000), Color.FromHSL ( 60.0/360.0, 1.000, 0.375)),
				Tuple.Create (new Color (0.000, 0.500, 0.000), Color.FromHSL (120.0/360.0, 1.000, 0.250)),
				Tuple.Create (new Color (0.500, 1.000, 1.000), Color.FromHSL (180.0/360.0, 1.000, 0.750)),
				Tuple.Create (new Color (0.500, 0.500, 1.000), Color.FromHSL (240.0/360.0, 1.000, 0.750)),
				Tuple.Create (new Color (0.750, 0.250, 0.750), Color.FromHSL (300.0/360.0, 0.500, 0.500)),
				Tuple.Create (new Color (0.628, 0.643, 0.142), Color.FromHSL ( 61.8/360.0, 0.638, 0.393)),
				Tuple.Create (new Color (0.255, 0.104, 0.918), Color.FromHSL (251.1/360.0, 0.832, 0.511)),
				Tuple.Create (new Color (0.116, 0.675, 0.255), Color.FromHSL (134.9/360.0, 0.707, 0.396)),
				Tuple.Create (new Color (0.941, 0.785, 0.053), Color.FromHSL ( 49.5/360.0, 0.893, 0.497)),
				Tuple.Create (new Color (0.704, 0.187, 0.897), Color.FromHSL (283.7/360.0, 0.775, 0.542)),
				Tuple.Create (new Color (0.931, 0.463, 0.316), Color.FromHSL ( 14.3/360.0, 0.817, 0.624)),
				Tuple.Create (new Color (0.998, 0.974, 0.532), Color.FromHSL ( 56.9/360.0, 0.991, 0.765)),
				Tuple.Create (new Color (0.099, 0.795, 0.591), Color.FromHSL (162.4/360.0, 0.779, 0.447)),
				Tuple.Create (new Color (0.211, 0.149, 0.597), Color.FromHSL (248.3/360.0, 0.601, 0.373)),
				Tuple.Create (new Color (0.495, 0.493, 0.721), Color.FromHSL (240.5/360.0, 0.290, 0.607)),
			};
			var s = 32;
			var canvas = Platforms.Current.CreateImageCanvas (new Size (s * tests.Length, s));

			foreach (var t in tests) {
				var rect = new Rect (0, 0, s, s);
				canvas.FillRectangle (rect, t.Item2);
				canvas.Translate (s, 0);

				AssertInRange (t.Item2.R, t.Item1.R - 1, t.Item1.R + 1);
				AssertInRange (t.Item2.G, t.Item1.G - 1, t.Item1.G + 1);
				AssertInRange (t.Item2.B, t.Item1.B - 1, t.Item1.B + 1);
			}

			await SaveImage (canvas, "Color.HSL.png");
		}

		static void AssertInRange (int x, int min, int max)
		{
			Assert.IsTrue (x >= min);
			Assert.IsTrue (x <= max);
		}

		[Test]
		public async Task HSB ()
		{
			// http://en.wikipedia.org/wiki/HSL_and_HSV#Examples
			var tests = new [] {
				Tuple.Create (new Color (1.000, 1.000, 1.000), Color.FromHSB (  0.0/360.0, 0.000, 1.000)),
				Tuple.Create (new Color (0.500, 0.500, 0.500), Color.FromHSB (  0.0/360.0, 0.000, 0.500)),
				Tuple.Create (new Color (0.000, 0.000, 0.000), Color.FromHSB (  0.0/360.0, 0.000, 0.000)),
				Tuple.Create (new Color (1.000, 0.000, 0.000), Color.FromHSB (  0.0/360.0, 1.000, 1.000)),
				Tuple.Create (new Color (0.750, 0.750, 0.000), Color.FromHSB ( 60.0/360.0, 1.000, 0.750)),
				Tuple.Create (new Color (0.000, 0.500, 0.000), Color.FromHSB (120.0/360.0, 1.000, 0.500)),
				Tuple.Create (new Color (0.500, 1.000, 1.000), Color.FromHSB (180.0/360.0, 0.500, 1.000)),
				Tuple.Create (new Color (0.500, 0.500, 1.000), Color.FromHSB (240.0/360.0, 0.500, 1.000)),
				Tuple.Create (new Color (0.750, 0.250, 0.750), Color.FromHSB (300.0/360.0, 0.667, 0.750)),
				Tuple.Create (new Color (0.628, 0.643, 0.142), Color.FromHSB ( 61.8/360.0, 0.779, 0.643)),
				Tuple.Create (new Color (0.255, 0.104, 0.918), Color.FromHSB (251.1/360.0, 0.887, 0.918)),
				Tuple.Create (new Color (0.116, 0.675, 0.255), Color.FromHSB (134.9/360.0, 0.828, 0.675)),
				Tuple.Create (new Color (0.941, 0.785, 0.053), Color.FromHSB ( 49.5/360.0, 0.944, 0.941)),
				Tuple.Create (new Color (0.704, 0.187, 0.897), Color.FromHSB (283.7/360.0, 0.792, 0.897)),
				Tuple.Create (new Color (0.931, 0.463, 0.316), Color.FromHSB ( 14.3/360.0, 0.661, 0.931)),
				Tuple.Create (new Color (0.998, 0.974, 0.532), Color.FromHSB ( 56.9/360.0, 0.467, 0.998)),
				Tuple.Create (new Color (0.099, 0.795, 0.591), Color.FromHSB (162.4/360.0, 0.875, 0.795)),
				Tuple.Create (new Color (0.211, 0.149, 0.597), Color.FromHSB (248.3/360.0, 0.750, 0.597)),
				Tuple.Create (new Color (0.495, 0.493, 0.721), Color.FromHSB (240.5/360.0, 0.316, 0.721)),
			};
			var s = 32;
			var canvas = Platforms.Current.CreateImageCanvas (new Size (s * tests.Length, s));

			foreach (var t in tests) {
				var rect = new Rect (0, 0, s, s);
				canvas.FillRectangle (rect, t.Item2);
				canvas.Translate (s, 0);

				AssertInRange (t.Item2.R, t.Item1.R - 1, t.Item1.R + 1);
				AssertInRange (t.Item2.G, t.Item1.G - 1, t.Item1.G + 1);
				AssertInRange (t.Item2.B, t.Item1.B - 1, t.Item1.B + 1);
			}

			await SaveImage (canvas, "Color.HSB.png");
		}
	}
}

