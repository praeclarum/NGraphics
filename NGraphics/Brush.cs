using System;

namespace NGraphics
{
	public abstract class Brush
	{
	}

	public static class Brushes
	{
		public static readonly SolidBrush Yellow = new SolidBrush (Colors.Yellow);
	}

	public class SolidBrush : Brush
	{
		public readonly Color Color;

		public SolidBrush (Color color)
		{
			Color = color;
		}
	}
}
