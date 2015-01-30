using System;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public class Graphic : IDrawable
	{
		public readonly List<IDrawable> Children = new List<IDrawable> ();

		public Size Size;
		public Rectangle ViewBox;

		public Graphic (Size size)
		{
			Size = size;
			ViewBox = new Rectangle (Point.Zero, size);
		}

		public void Draw (ICanvas s)
		{
			foreach (var c in Children) {
				c.Draw (s);
			}
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

	public delegate void DrawingFunc (ICanvas surface);

	public class Drawing
	{
		readonly Size size;
		readonly DrawingFunc func;

		Graphic graphic = null;

		public Graphic Graphic {
			get {
				if (graphic == null) {
					try {
						DrawGraphic ();
					} catch (Exception ex) {
						graphic = new Graphic (size);
						Log.Error (ex);
					}
				}
				return graphic;
			}
		}

		public Drawing (Size size, DrawingFunc func)
		{
			if (func == null) {
				throw new ArgumentNullException ("func");
			}
			this.size = size;
			this.func = func;
		}

		public void Invalidate ()
		{
			graphic = null;
		}

		void DrawGraphic ()
		{
			var c = new GraphicCanvas (size);
			if (func != null)
				func (c);
			graphic = c.Graphic;
		}
	}
}
