using System;
using System.Collections.Generic;
using System.Linq;

namespace XGraphics
{
	public delegate void DrawingFunc (ISurface surface);

	public class Drawing : ISurface, IDrawable
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

		public Drawing (DrawingFunc func)
		{
			this.func = func;
		}

		public void Invalidate ()
		{
			children.Clear ();
			isValid = false;
		}

		public void DrawOval (Point position, Size size, Pen pen = null, Brush brush = null)
		{
			children.Add (new Oval (position, size, pen, brush));
		}

		void DrawIfNeeded ()
		{
			if (!isValid) {
				if (func != null)
					func (this);
				isValid = true;
			}
		}

		public void Draw (ISurface s)
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
