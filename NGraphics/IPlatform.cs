using System;
using System.Linq;
using System.Reflection;

namespace NGraphics
{
	public interface IPlatform
	{
		string Name { get; }
		IImageCanvas CreateImageSurface (int pixelWidth, int pixelHeight, bool transparency = true);
	}
}

