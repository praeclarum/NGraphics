using NUnit.Framework;
using System;
using NGraphics;
using System.Diagnostics;

namespace NGraphics.Test
{
	public class PlatformTest
	{
		public static class Platforms
		{
			public static IPlatform Current { get { return PlatformTest.Platform; } }
		}

		public static IPlatform Platform = new NullPlatform ();

		public static string ResultsDirectory = "";
		public static string GetPath (string filename)
		{
			var name = (filename.EndsWith (".png", StringComparison.Ordinal)) ?
				System.IO.Path.GetFileNameWithoutExtension (filename) :
				filename;
			var path = System.IO.Path.Combine (ResultsDirectory, name + "-" + Platform.Name + ".png");
			Debug.WriteLine (path);
			return path;
		}
	}

	[TestFixture]
	public class ImageCanvasTest : PlatformTest
	{
		[Test]
		public void TriWithRadGrad ()
		{
			var canvas = Platform.CreateImageCanvas (new Size (100), transparency: true);
			var size = new Size (100);
			var b = new RadialGradientBrush (
				new Point (0.5, 1), 1,
				Colors.Yellow, Colors.Blue);
			var p = new Path ();
			p.MoveTo (0, 0);
			p.LineTo (size.Width, 0);
			p.LineTo (size.Width / 2, size.Height);
			p.Close ();
			p.Brush = b;
			p.Draw (canvas);
			canvas.GetImage ().SaveAsPng (GetPath ("ImageCanvas.TriWithRadGrad"));
		}
		[Test]
		public void Line ()
		{
			var canvas = Platform.CreateImageCanvas (new Size (100), transparency: true);
			canvas.DrawLine (10, 20, 80, 70, Colors.DarkGray, 5);
			canvas.GetImage ().SaveAsPng (GetPath ("ImageCanvas.Line"));
		}
		[Test]
		public void Ellipse ()
		{
			var p = Platform;
			var s = p.CreateImageCanvas (new Size (100), transparency: true);
			s.DrawEllipse (new Point (10, 20), new Size (30, 40), Pens.Red.WithWidth (10), Brushes.Yellow);
			var i = s.GetImage ();
			var path = GetPath ("Ellipse");
			i.SaveAsPng (path);
		}
		[Test]
		public void EllipseWithoutBackground ()
		{
			var p = Platform;
			var s = p.CreateImageCanvas (new Size (100), transparency: false);
			s.DrawEllipse (new Point (10, 20), new Size (30, 40), Pens.Red.WithWidth (10), Brushes.Yellow);
			var i = s.GetImage ();
			var path = GetPath ("ImageCanvas.EllipseWithoutBackground");
			i.SaveAsPng (path);
		}
		[Test]
		public void EllipseWithBackground ()
		{
			var p = Platform;
			var s = p.CreateImageCanvas (new Size (100), transparency: false);
			s.FillRectangle (new Rect (new Size (100)), Colors.DarkGray);
			s.DrawEllipse (new Point (10, 20), new Size (30, 40), Pens.Red.WithWidth (10), Brushes.Yellow);
			var i = s.GetImage ();
			var path = GetPath ("ImageCanvas.EllipseWithBackground");
			i.SaveAsPng (path);
		}
	}
}

