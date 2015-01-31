using System;
using System.Linq;
using System.Reflection;

namespace NGraphics
{
	public interface IPlatform
	{
		string Name { get; }
		IImageCanvas CreateImageCanvas (Size size, double scale = 1.0, bool transparency = true);
	}
}

