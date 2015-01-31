using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	public class Group : Element
	{
		public readonly List<IDrawable> Children = new List<IDrawable> ();

		public Group ()
			: base (null, null)
		{			
		}

		protected override void DrawElement (ICanvas canvas)
		{
			foreach (var c in Children) {
				c.Draw (canvas);
			}
		}
	}
	
}
