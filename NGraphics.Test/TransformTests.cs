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
	}
}

