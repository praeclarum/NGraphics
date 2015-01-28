using NUnit.Framework;
using System;
using NGraphics;

namespace NGraphics.Test
{
	[TestFixture]
	public class ImageSurfaceTest
	{
		[Test]
		public void Oval ()
		{
			var p = Platforms.Current;
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

