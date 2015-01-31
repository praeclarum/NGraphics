using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	public abstract class PathCommand
	{
	}
	public class MoveTo : PathCommand
	{
		public Point AbsolutePoint;
		public MoveTo (Point absolutePoint)
		{
			AbsolutePoint = absolutePoint;
		}
	}
	public class LineTo : PathCommand
	{
		public Point AbsolutePoint;
		public LineTo (Point absolutePoint)
		{
			AbsolutePoint = absolutePoint;
		}
	}
	public class CurveTo : PathCommand
	{
	}
	public class ClosePath : PathCommand
	{
	}

	public class Path : IDrawable
	{
		Pen pen;
		Brush brush;

		public readonly List<PathCommand> Commands = new List<PathCommand> ();

		public bool IsClosed {
			get {
				return Commands.Count > 0 && Commands [Commands.Count - 1] is ClosePath;
			}
		}

		public Path (IEnumerable<PathCommand> commands, Pen pen = null, Brush brush = null)
		{
			Commands.AddRange (commands);
			this.pen = pen;
			this.brush = brush;
		}
		public Path (Pen pen = null, Brush brush = null)
		{
			this.pen = pen;
			this.brush = brush;
		}

		public void Draw (ICanvas canvas)
		{
			canvas.DrawPath (Commands, pen, brush);
		}

		void Add (PathCommand cmd)
		{
			if (IsClosed)
				return;
			Commands.Add (cmd);
		}

		public void MoveTo (Point point)
		{
			Add (new MoveTo (point));
		}

		public void LineTo (Point point)
		{
			Add (new LineTo (point));
		}

		public void Close ()
		{
			Add (new ClosePath ());
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Path ([...])");
		}
	}
}

