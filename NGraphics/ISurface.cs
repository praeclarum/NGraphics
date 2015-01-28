using System;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public interface ISurface
	{
		void DrawOval (Rectangle frame, Pen pen = null, Brush brush = null);
	}

	public static class SurfaceEx
	{
		public static void DrawOval (this ISurface surface, Point position, Size size, Pen pen = null, Brush brush = null)
		{
			surface.DrawOval (new Rectangle (position, size), pen, brush);
		}
	}
}
