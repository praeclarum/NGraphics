using System;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public interface IDrawable
	{
		void Draw (ICanvas canvas);
	}
}
