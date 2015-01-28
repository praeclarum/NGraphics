using System;
using System.Collections.Generic;
using System.Linq;

namespace XGraphics
{
	public interface ISurface
	{
		void DrawOval (Point position, Size size, Pen pen = null, Brush brush = null);
	}
}
