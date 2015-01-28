using System;

namespace NGraphics.SystemDrawing
{
	public interface IPlatform
	{
		IImageSurface CreateImageSurface (int pixelWidth, int pixelHeight, bool transparency = true);
	}
}

