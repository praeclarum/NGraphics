using NUnit.Framework;
using System;
using NGraphics.SystemDrawing;

namespace NGraphics.Test
{
	[TestFixture]
	public class SystemDrawingTest
	{
		[Test]
		public void Oval ()
		{
			var p = new SystemDrawingPlatform ();
			var s = p.CreateImageSurface (100, 100);
			s.DrawOval (new Point (10, 20), new Size (30, 40), Pens.Red.WithWidth (10), Brushes.Yellow);
			var i = s.GetImage ();
			var path = System.IO.Path.GetTempFileName () + ".png";
			i.SaveAsPng (path);
			Console.WriteLine (path);
			System.Diagnostics.Process.Start ("open", "\"" + path + "\"");
		}
	}
}

