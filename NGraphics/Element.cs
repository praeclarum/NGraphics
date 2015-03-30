using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	public abstract class Element : IDrawable
	{
		public string Id { get; set; }
		public Transform Transform { get; set; }
		public Pen Pen { get; set; }
		public Brush Brush { get; set; }

		public Element (Pen pen, Brush brush)
		{
			Id = Guid.NewGuid ().ToString ();
			Pen = pen;
			Brush = brush;
			Transform = NGraphics.Transform.Identity;
		}

		protected abstract void DrawElement (ICanvas canvas);

		#region IDrawable implementation

		public void Draw (ICanvas canvas)
		{
			var t = Transform;
			var pushedState = false;
			try {
				if (t != NGraphics.Transform.Identity) {
					canvas.SaveState ();
					pushedState = true;
					canvas.Transform (t);
				}
				DrawElement (canvas);
			} finally {
				if (pushedState) {
					canvas.RestoreState ();
				}
			}
		}

		#endregion
	}


}
