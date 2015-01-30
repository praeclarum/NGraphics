using System;
using System.Collections.Generic;
using System.Linq;

namespace NGraphics
{
	public delegate void DrawingFunc (ICanvas surface);

	/// <summary>
	/// A drawing is a surface that remembers what was drawn onto it.
	/// You can also give it a function that does the drawing lazily and automatically.
	/// Drawings can be drawn to other surfaces or saved to a file.
	/// </summary>
	public class Graphic : ICanvas, IDrawable
	{
		bool isValid = false;

		readonly List<IDrawable> children = new List<IDrawable> ();

		readonly DrawingFunc func;

		public int NumChildren {
			get {
				try {
					DrawIfNeeded ();
				} catch (Exception ex) {
					Log.Error (ex);
				}
				return children.Count;
			}
		}

		public Graphic (DrawingFunc func = null)
		{
			this.func = func;
		}

		/// <summary>
		/// Clears the drawing and then lazily runs the drawing function (if any).
		/// </summary>
		public void Refresh ()
		{
			children.Clear ();
			isValid = false;
		}

		public void DrawOval (Rectangle frame, Pen pen = null, Brush brush = null)
		{
			children.Add (new Ellipse (frame, pen, brush));
		}

		void DrawIfNeeded ()
		{
			if (!isValid) {
				if (func != null)
					func (this);
				isValid = true;
			}
		}

		public void Draw (ICanvas s)
		{
			foreach (var c in children) {
				c.Draw (s);
			}
		}

		public override string ToString ()
		{
			try {
				if (children.Count == 0)
					return "Drawing";
				var w =
					children.
					GroupBy (x => x.GetType ().Name).
					Select (x => x.Count () + " " + x.Key);
				return "Drawing with " + string.Join (", ", w);
			} catch {
				return "Drawing with errors!";
			}
		}
	}
}
