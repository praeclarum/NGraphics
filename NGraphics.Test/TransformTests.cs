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
using System.Threading.Tasks;

namespace NGraphics.Test
{
	[TestFixture]
	public class TransformTests : PlatformTest
	{
		[Test]
		public async Task RotateTranslate ()
		{
			var canvas = Platforms.Current.CreateImageCanvas (new Size (200));

			canvas.Rotate (30);
			canvas.Translate (50, 50);

			canvas.DrawRectangle (0, 0, 150, 75, brush: Brushes.Red);

			await SaveImage (canvas, "TransformRotateTranslate.png");
		}

		[Test]
		public async Task TranslateRotate ()
		{
			var canvas = Platforms.Current.CreateImageCanvas (new Size (200));

			canvas.Translate (50, 50);
			canvas.Rotate (30);

			canvas.DrawRectangle (0, 0, 150, 75, brush: Brushes.Red);

			await SaveImage (canvas, "TransformTranslateRotate.png");
		}

		[Test]
		public void VectorOnTheRight ()
		{
			var t = Transform.Rotate (45) * Transform.Translate(9, 0);
			var p0 = new Point (1, 0);
			var yp0 = t.TransformPoint (p0);
			ClassicAssert.AreEqual (7.071, yp0.X, 0.001);
			ClassicAssert.AreEqual (7.071, yp0.Y, 0.001);
		}

		[Test]
		public void VectorOnTheRight2 ()
		{
			var t = Transform.Translate (9, 0) * Transform.Rotate (45);
			var p0 = new Point (1, 0);
			var yp0 = t.TransformPoint (p0);
			ClassicAssert.AreEqual (9.7071, yp0.X, 0.001);
			ClassicAssert.AreEqual (0.7071, yp0.Y, 0.001);
		}
	}
}

