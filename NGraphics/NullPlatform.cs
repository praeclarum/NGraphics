using System;

namespace NGraphics
{
	public class NullPlatform : IPlatform
	{
		public string Name { get { return "Null"; } }

		public IImageSurface CreateImageSurface (int pixelWidth, int pixelHeight, bool transparency = true)
		{
			return new NullImageSurface ();
		}

		class NullImageSurface : IImageSurface
		{
			public IImage GetImage ()
			{
				return new NullImage ();
			}
			public void DrawOval (Rectangle frame, Pen pen = null, Brush brush = null)
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
