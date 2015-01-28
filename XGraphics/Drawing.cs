using System;
using System.Collections.Generic;

namespace XGraphics
{
	public delegate void DrawingFunc (ISurface surface);

	public class Drawing : ISurface
	{
		bool isValid = false;

		List<object> children = new List<object> ();

		DrawingFunc func;

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
	}

	public interface ISurface
	{
		void DrawOval (Point position, Size size, Pen pen = null, Brush brush = null);
	}
}

