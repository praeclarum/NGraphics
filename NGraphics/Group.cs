using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	public class Group : IDrawable
	{
		public readonly List<IDrawable> Children = new List<IDrawable> ();

		public Transform Transform;

		public void Draw (ICanvas canvas)
		{
			var t = Transform;
			if (t != null) {
				canvas.SaveState ();
				canvas.Transform (t);
			}
			foreach (var c in Children) {
				c.Draw (canvas);
			}
			if (t != null) {
				canvas.RestoreState ();
			}
		}
	}
	
}
