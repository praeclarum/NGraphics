using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace NGraphics.SystemDrawing
{
	public interface IImage //: IDrawable
	{
		void SaveAsPng (string path);
	}	

	public interface IImageSurface : ISurface
	{
		IImage GetImage ();
	}
}
