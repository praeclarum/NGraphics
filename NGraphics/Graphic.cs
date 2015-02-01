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

		public void Draw (ICanvas canvas)
		{
			canvas.SaveState ();

			//
			// Scale the viewBox into the size
			//
			var sx = 1.0;
			if (ViewBox.Width > 0) {
				sx = Size.Width / ViewBox.Width;
			}
			var sy = 1.0;
			if (ViewBox.Height > 0) {
				sy = Size.Height / ViewBox.Height;
			}

			canvas.Scale (sx, sy);
			canvas.Translate (-ViewBox.X, -ViewBox.Y);

			//
			// Draw
			//
			foreach (var c in Children) {
				c.Draw (canvas);
			}

			canvas.RestoreState ();
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
