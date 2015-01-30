using System;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public class Graphic : IDrawable
	{
		public readonly List<IDrawable> Children = new List<IDrawable> ();

		public Size Size;
		public Rect ViewBox;
		public string Title = "";
		public string Description = "";

		public Graphic (Size size, Rect viewBox)
		{
			Size = size;
			ViewBox = viewBox;
		}

		public Graphic (Size size)
			: this (size, new Rect (Point.Zero, size))
		{
		}

		public void Draw (ICanvas s)
		{
			foreach (var c in Children) {
				c.Draw (s);
			}
		}

		public static Graphic LoadSvg (System.IO.TextReader reader)
		{
			var svgr = new SvgReader (reader);
			return svgr.Graphic;
		}

		public override string ToString ()
		{
			try {
				if (Children.Count == 0)
					return "Graphic";
				var w =
					Children.
					GroupBy (x => x.GetType ().Name).
					Select (x => x.Count () + " " + x.Key);
				return "Graphic with " + string.Join (", ", w);
			} catch {
				return "Graphic with errors!";
			}
		}
	}
}
