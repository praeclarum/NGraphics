using System;

namespace NGraphics
{
	public class Font
	{
		public string Family = "Georgia";
		public double Size = 16;

		public string Name { get { return Family; } }
	}

	public enum TextAlignment
	{
		Left,
		Center,
		Right
	}
}

