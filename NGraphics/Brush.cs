using System;

namespace NGraphics
{
	public abstract class Brush
	{
	}

	public static class Brushes
	{
		public static readonly SolidBrush Black = new SolidBrush (Colors.Black);
		public static readonly SolidBrush Gray = new SolidBrush (Colors.Gray);
		public static readonly SolidBrush White = new SolidBrush (Colors.White);
		public static readonly SolidBrush Red = new SolidBrush (Colors.Red);
		public static readonly SolidBrush Yellow = new SolidBrush (Colors.Yellow);
		public static readonly SolidBrush Green = new SolidBrush (Colors.Green);
		public static readonly SolidBrush Blue = new SolidBrush (Colors.Blue);
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
