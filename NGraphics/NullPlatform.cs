using System;
using System.Collections.Generic;

namespace NGraphics
{
	public class NullPlatform : IPlatform
	{
		public string Name { get { return "Null"; } }

		public IImageCanvas CreateImageCanvas (Size size, double scale = 1.0, bool transparency = true)
		{
			return new NullImageSurface ();
		}

		class NullImageSurface : IImageCanvas
		{
			public IImage GetImage ()
			{
				return new NullImage ();
			}
			public void SaveState ()
			{
			}
			public void Transform (Transform transform)
			{
			}
			public void RestoreState ()
			{
			}
			public void DrawText (Point point, string text, Pen pen = null, Brush brush = null)
			{
			}
			public void DrawPath (IEnumerable<PathOp> ops, Pen pen = null, Brush brush = null)
			{
			}
			public void DrawRectangle (Rect frame, Pen pen = null, Brush brush = null)
			{
			}
			public void DrawEllipse (Rect frame, Pen pen = null, Brush brush = null)
			{
			}
		}

		class NullImage : IImage
		{
			public void SaveAsPng (string path)
			{
			}
		}
	}
}
