using System;

namespace NGraphics
{
	public class NullPlatform : IPlatform
	{
		public string Name { get { return "Null"; } }

		public IImageCanvas CreateImageSurface (int pixelWidth, int pixelHeight, bool transparency = true)
		{
			return new NullImageSurface ();
		}

		class NullImageSurface : IImageCanvas
		{
			public IImage GetImage ()
			{
				return new NullImage ();
			}
			public void DrawRectangle (Rect frame, Pen pen = null, Brush brush = null)
			{
			}
			public void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null)
			{
			}
		}

		class NullImage : IImage
		{
			public void SaveAsPng (string path)
			{
			}
		}
	}
}
