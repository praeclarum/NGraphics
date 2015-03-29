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
	public class BrushTests : PlatformTest
	{
		[Test]
		public void RectLinearGradient ()
		{
			var canvas = Platforms.Current.CreateImageCanvas (new Size (100));

			var rect = new Rect (0, 10, 100, 80);
			var brush = new LinearGradientBrush (
				Point.Zero,
				Point.OneY,
				Colors.Green,
				Colors.LightGray);				

			canvas.DrawRectangle (rect, brush: brush);

			canvas.GetImage ().SaveAsPng (GetPath ("Brush.RectLinearGradient.png"));
		}

		[Test]
		public void RectAbsLinearGradient ()
		{
			var canvas = Platforms.Current.CreateImageCanvas (new Size (100));

			var rect = new Rect (0, 10, 100, 80);
			var brush = new LinearGradientBrush (
				Point.Zero,
				new Point (0, 200),
				Colors.Yellow,
				Colors.Red);				
			brush.Absolute = true;

			canvas.DrawRectangle (rect, brush: brush);

			canvas.GetImage ().SaveAsPng (GetPath ("Brush.RectAbsLinearGradient.png"));
		}
	}
}

