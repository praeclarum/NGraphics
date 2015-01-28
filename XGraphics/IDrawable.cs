using System;
using System.Collections.Generic;
using System.Linq;

namespace XGraphics
{
	public interface IDrawable
	{
		void Draw (ISurface surface);
	}
}
