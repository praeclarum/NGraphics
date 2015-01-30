using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	public class Group : IDrawable
	{
		public readonly List<IDrawable> Children = new List<IDrawable> ();

		public void Draw (ICanvas surface)
		{
			foreach (var c in Children) {
				c.Draw (surface);
			}
		}
	}
	
}
