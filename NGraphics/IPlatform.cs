using System;
using System.Linq;
using System.Reflection;

namespace NGraphics
{
	public interface IPlatform
	{
		string Name { get; }
		IImageSurface CreateImageSurface (int pixelWidth, int pixelHeight, bool transparency = true);
	}
}

