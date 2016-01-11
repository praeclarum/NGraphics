using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NGraphics
{
	public class NullPlatform : IPlatform
	{
		public string Name { get { return "Null"; } }

		public TextMetrics MeasureText (string text, Font font)
		{
			return new TextMetrics ();
		}

		public Task<Stream> OpenFileStreamForWritingAsync (string path)
		{
			return Task.FromResult ((Stream)new MemoryStream ());
		}

		public IImageCanvas CreateImageCanvas (Size size, double scale = 1.0, bool transparency = true)
		{
			return new NullImageSurface ();
		}

		public IImage CreateImage (Color[] colors, int width, double scale = 1.0)
		{
			return new NullImage ();
		}

		public IImage LoadImage (Stream stream)
		{
			return new NullImage ();
		}
		public IImage LoadImage (string path)
		{
			return new NullImage ();
		}

		class NullImageSurface : IImageCanvas
		{
			public IImage GetImage ()
			{
				return new NullImage ();
			}
			public Size Size { get { return Size.Zero; } }
			public double Scale { get { return 1.0; } }
			public void SaveState ()
			{
			}
			public void Transform (Transform transform)
			{
			}
			public void RestoreState ()
			{
			}
			public TextMetrics MeasureText(string text, Font font)
			{
				return new TextMetrics ();
			}
			public void DrawText (string text, Rect frame, Font font, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null)
			{
			}
			public void DrawPath (IEnumerable<PathOp> ops, Pen pen = null, Brush brush = null)
			{
			}
			public void DrawRectangle (Rect frame, Size corner, Pen pen = null, Brush brush = null)
			{
			}
			public void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null)
			{
			}
			public void DrawImage (IImage image, Rect frame, double alpha = 1.0)
			{
			}
		}

		class NullImage : IImage
		{
			public Size Size { get { return Size.Zero; } }
			public double Scale { get { return 1.0; } }
			public void SaveAsPng (string path)
			{
			}
			public void SaveAsPng (Stream stream)
			{
			}
		}
	}
}
