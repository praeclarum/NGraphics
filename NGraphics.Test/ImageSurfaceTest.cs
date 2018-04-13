#if VSTEST
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#else
using NUnit.Framework;
#endif
using System;
using NGraphics;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace NGraphics.Test
{
	public class PlatformTest
	{
		public static class Platforms
		{
			public static IPlatform Current { get { return PlatformTest.Platform; } }
		}

		public static IPlatform Platform =
#if NETFX_CORE
			new WindowsXamlPlatform ();
#else
			new NullPlatform ();
#endif

		public static string ResultsDirectory = "";
		public static string GetPath (string filename, string ext)
		{
			var name = (filename.EndsWith (".png", StringComparison.Ordinal)) ?
				System.IO.Path.GetFileNameWithoutExtension (filename) :
				filename;
			var path = System.IO.Path.Combine (ResultsDirectory, name + "-" + Platform.Name + ext);
			Debug.WriteLine (path);
			return path;
		}
		public Stream OpenResource (string path)
		{
			if (string.IsNullOrEmpty (path))
				throw new ArgumentException ("path");
			var ty = typeof(AssemblyMarker);
			var ti = ty.GetTypeInfo ();
			var assembly = ti.Assembly;
			var resources = assembly.GetManifestResourceNames ();
			return assembly.GetManifestResourceStream ("NGraphics.Test.Inputs." + path);
		}
		public IImage GetResourceImage (string name)
		{
			using (var s = OpenResource (name)) {
				return Platform.LoadImage (s);
			}
		}

		public static Func<string, Stream> OpenStream = p => null;
		public static Func<Stream, Task> CloseStream = s => Task.FromResult<object> (null);

		public async Task SaveImage (IImage i, string name)
		{
			var path = GetPath (name, ".png");
			using (var s = OpenStream (path)) {
				if (s == null) {
					i.SaveAsPng (path);
				}
				else {
					i.SaveAsPng (s);
					await CloseStream (s);
				}
			}
		}

		public Task SaveImage (IImageCanvas canvas, string name)
		{
			return SaveImage (canvas.GetImage (), name);
		}

		public async Task SaveSvg (GraphicCanvas canvas, string name)
		{
			var path = GetPath (name, ".svg");
			using (var s = OpenStream (path)) {
				if (s == null) {
					using (var ss = await Platforms.Current.OpenFileStreamForWritingAsync (path)) {
						using (var w = new System.IO.StreamWriter (ss)) {
							canvas.Graphic.WriteSvg (w);
						}
					}
				}
				else {
					var w = new System.IO.StreamWriter (s);
					canvas.Graphic.WriteSvg (w);
					await w.FlushAsync ();
					await CloseStream (s);
				}
			}
		}
	}

	[TestFixture]
	public class ImageCanvasTest : PlatformTest
	{
		[Test]
		public async Task BlurImage ()
		{
			var img = Platform.CreateImage (
				          new[] { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow },
				          2);
			var canvas = Platform.CreateImageCanvas (new Size (100), transparency: true);
			canvas.DrawImage (img, new Rect (new Size (100)));
			await SaveImage (canvas, "ImageCanvas.BlurImage");
		}

		[Test]
		public async Task BlurImage2 ()
		{
			var img = Platform.CreateImage (
				new[] { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow },
				2);
			var canvas = Platform.CreateImageCanvas (new Size (200, 100), transparency: true);
			canvas.DrawImage (img, new Rect (new Size (50)));
			canvas.DrawImage (img, new Rect (new Point (0, 50), new Size (50)));
			canvas.DrawImage (img, new Rect (new Point (50, 0), new Size (150, 50)));
			canvas.DrawImage (img, new Rect (new Point (50, 50), new Size (150, 50)));
			await SaveImage (canvas, "ImageCanvas.BlurImage2");
		}

		[Test]
		public async Task Cats ()
		{
			var img = GetResourceImage ("cat.png");
			var canvas = Platform.CreateImageCanvas (new Size (100, 200), transparency: true);
			canvas.DrawImage (img, new Rect (new Size (50)));
			canvas.DrawImage (img, new Rect (new Point (50, 0), new Size (50)));
			canvas.DrawImage (img, new Rect (new Point (0, 50), new Size (50, 150)));
			canvas.DrawImage (img, new Rect (new Point (50, 50), new Size (50, 150)));
			await SaveImage (canvas, "ImageCanvas.Cats");
		}

		[Test]
		public async Task DrawImageWithAlpha ()
		{
			var img = GetResourceImage ("cat.png");
			var canvas = Platform.CreateImageCanvas (new Size (100, 200), transparency: true);
			canvas.DrawImage (img, new Rect (new Size (50)), 1);
			canvas.DrawImage (img, new Rect (new Point (50, 0), new Size (50)), 0.5);
			canvas.DrawImage (img, new Rect (new Point (0, 50), new Size (50, 150)), 0.25);
			canvas.DrawImage (img, new Rect (new Point (50, 50), new Size (50, 150)), 0);
			await SaveImage (canvas, "ImageCanvas.DrawImageWithAlpha");
		}

		[Test]
		public async Task TriWithRadGrad ()
		{
			var canvas = Platform.CreateImageCanvas (new Size (100), transparency: true);
			var size = new Size (100);
			var b = new RadialGradientBrush (
				new Point (0.5, 1), new Size (1),
				Colors.Yellow, Colors.Blue);
			var p = new Path ();
			p.MoveTo (0, 0);
			p.LineTo (size.Width, 0);
			p.LineTo (size.Width / 2, size.Height);
			p.Close ();
			p.Brush = b;
			p.Draw (canvas);
			await SaveImage (canvas, "ImageCanvas.TriWithRadGrad");
		}
		[Test]
		public async Task Line ()
		{
			var canvas = Platform.CreateImageCanvas (new Size (100), transparency: true);
			canvas.DrawLine (10, 20, 80, 70, Colors.DarkGray, 5);
			await SaveImage (canvas, "ImageCanvas.Line");
		}
		[Test]
		public async Task Ellipse ()
		{
			var p = Platform;
			var s = p.CreateImageCanvas (new Size (100), transparency: true);
			s.DrawEllipse (new Point (10, 20), new Size (30, 40), Pens.Red.WithWidth (10), Brushes.Yellow);
			await SaveImage (s, "Ellipse");
		}
		[Test]
		public async Task EllipseWithoutBackground ()
		{
			var p = Platform;
			var s = p.CreateImageCanvas (new Size (100), transparency: false);
			s.DrawEllipse (new Point (10, 20), new Size (30, 40), Pens.Red.WithWidth (10), Brushes.Yellow);
			await SaveImage (s, "ImageCanvas.EllipseWithoutBackground");
		}
		[Test]
		public async Task EllipseWithBackground ()
		{
			var p = Platform;
			var s = p.CreateImageCanvas (new Size (100), transparency: false);
			s.FillRectangle (new Rect (new Size (100)), Colors.DarkGray);
			s.DrawEllipse (new Point (10, 20), new Size (30, 40), Pens.Red.WithWidth (10), Brushes.Yellow);
			await SaveImage (s, "ImageCanvas.EllipseWithBackground");
		}
	}
}

