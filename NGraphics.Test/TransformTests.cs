#if VSTEST
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#else
using NUnit.Framework;
#endif
using System.IO;
using System;
using System.Reflection;

namespace NGraphics.Test
{
	[TestFixture]
	public class TransformTests : PlatformTest
	{
		[Test]
		public void RotateTranslate ()
		{
			var canvas = Platforms.Current.CreateImageCanvas (new Size (200));

			canvas.Rotate (30);
			canvas.Translate (50, 50);

			canvas.DrawRectangle (0, 0, 150, 75, brush: Brushes.Red);

			canvas.GetImage ().SaveAsPng (GetPath ("TransformRotateTranslate.png"));
		}

		[Test]
		public void TranslateRotate ()
		{
			var canvas = Platforms.Current.CreateImageCanvas (new Size (200));

			canvas.Translate (50, 50);
			canvas.Rotate (30);

			canvas.DrawRectangle (0, 0, 150, 75, brush: Brushes.Red);

			canvas.GetImage ().SaveAsPng (GetPath ("TransformTranslateRotate.png"));
		}
	}
}

