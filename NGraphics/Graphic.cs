using System;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public class Graphic : IDrawable
	{
		public readonly List<IDrawable> Children = new List<IDrawable> ();

		public Size Size;
		public Rectangle ViewPort;

		public Graphic (Size size, Rectangle viewPort)
		{
			Size = size;
			ViewPort = viewPort;
		}

		public Graphic (Size size)
			: this (size, new Rectangle (Point.Zero, size))
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
