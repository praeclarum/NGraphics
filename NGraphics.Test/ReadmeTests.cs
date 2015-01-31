using NUnit.Framework;
using System.IO;
using System;
using System.Reflection;

namespace NGraphics.Test
{
	[TestFixture]
	public class ReadmeTests : PlatformTest
	{
		[Test]
		public void Example1 ()
		{
			var canvas = Platforms.Current.CreateImageCanvas (new Size (100), scale: 2);

			canvas.DrawEllipse (10, 20, 30, 30, Pens.Red, Brushes.White);
			canvas.DrawRectangle (40, 50, 60, 70, brush: Brushes.Blue);
			canvas.DrawPath (new PathOp[] {	
				new MoveTo (100, 100),
				new LineTo (50, 100),
				new LineTo (50, 0),
				new ClosePath ()
			}, brush: Brushes.Gray);

			canvas.GetImage ().SaveAsPng (GetPath ("Example1.png"));
		}
	}
}

